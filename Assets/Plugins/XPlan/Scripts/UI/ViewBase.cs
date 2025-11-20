using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using XPlan.UI.Fade;

namespace XPlan.UI
{
    // 標記此欄位可與ViewModel成員繫結（預設由欄位名推導）
    // 名稱為 BindName
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class BindNameAttribute : Attribute
    {
        public string Name { get; }
        public BindNameAttribute(string name) => Name = name;
    }

    // 標記此欄位要參與「可見性綁定」(Visible) 的掃描。
    // 可選 name 參數可覆寫預設的 DeriveBaseName(f.Name)。
    // 名稱為 BindVisibleTarget
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public sealed class BindVisibleTargetAttribute : Attribute
    {
        public string Name { get; }
        public BindVisibleTargetAttribute(string name = null) => Name = name;
    }

    public sealed class ObservableBinding
    {
        public object OpInstance;       // ObservableProperty<T> 實體
        public Type ValueType;          // T
        public PropertyInfo ValueProp;  // .Value
        public MethodInfo ForceNotify;
    }

    public class ViewBase<TViewModel> : MonoBehaviour, IUIView where TViewModel : ViewModelBase
    {
        private TViewModel _viewModel;                                                                          // viewmodel本體
        private readonly List<IDisposable> _disposables                         = new();                        // 解除訂閱集中管理
        private readonly Dictionary<string, ObservableBinding> _vmObservableMap = new(StringComparer.Ordinal);  // 新增：把 VM 內的 ObservableProperty 索引起來（baseName → 綁定資訊）
        private const int TimeToWaitViewModel                                   = 5000;        
        

        private void Awake()
        {
            VMLocator.GetOrWaitAsync<TViewModel>(TimeToWaitViewModel, (vm) => 
            {
                _viewModel = vm;

                // 先建立 VM 的 Observable 索引（UI→VM 要用）
                IndexVmObservables();

                // 再自動註冊 UI 控制的事件（UI→VM）
                AutoRegisterComponents();

                // 最後綁訂閱（VM→UI）
                AutoBindObservables();

                // ★ 新增：VM→UI（Visible）
                AutoBindVisibility();   
            });
        }

        private void OnDestroy()
        {
            foreach (var d in _disposables)
            {
                d?.Dispose();
            }

            _disposables.Clear();

            // 清掉動態產的 Sprite（僅清理我們產生的）
            foreach (var kv in _spriteFromTexCache)
            {
                if (kv.Value != null) Destroy(kv.Value);
            }
            _spriteFromTexCache.Clear();
        }

        public static async Task<T> WithTimeout<T>(Task<T> task, int timeoutMs)
        {
            using (var cts = new CancellationTokenSource())
            {
                var delay       = Task.Delay(timeoutMs, cts.Token);
                var finished    = await Task.WhenAny(task, delay);

                if (finished == delay)
                    throw new TimeoutException();

                cts.Cancel(); // 終止 delay 任務
                return await task;
            }
        }

        /*******************************************
         * 依照Components名稱去綁定ViewModel的函數
         * *****************************************/

        protected void TryBindButton(Button btn, string methodName)
        {
            if (btn == null) return;

            if (TryResolveMethod(methodName, Type.EmptyTypes, out var target, out var mi))
            {
                btn.onClick.AddListener(() => mi.Invoke(target, null));
            }
            else
            {
                Debug.LogWarning($"[{name}] 找不到方法 {methodName}() 於 View 或 ViewModel。");
            }
        }

        protected void TryBindToggle(Toggle toggle, string methodName)
        {
            if (toggle == null) return;

            // 期望簽名：void OnXxxChange(bool)
            if (TryResolveMethod(methodName, new[] { typeof(bool) }, out var target, out var mi))
            {
                toggle.onValueChanged.AddListener(v => mi.Invoke(target, new object[] { v }));
            }
            else
            {
                Debug.LogWarning($"[{name}] 找不到方法 {methodName}(bool) 於 View 或 ViewModel。");
            }
        }

        protected void TryBindInputField(InputField tf, string methodName)
        {
            if (tf == null) return;

            // 期望簽名：void OnXxxChange(string)
            if (TryResolveMethod(methodName, new[] { typeof(string) }, out var target, out var mi))
            {
                tf.onValueChanged.AddListener(s => mi.Invoke(target, new object[] { s }));
            }
            else
            {
                Debug.LogWarning($"[{name}] 找不到方法 {methodName}(string) 於 View 或 ViewModel。");
            }
        }

        protected void TryBindSlider(Slider slider, string methodName)
        {
            if (slider == null) return;

            // 期望簽名：void OnXxxChange(float)
            if (TryResolveMethod(methodName, new[] { typeof(float) }, out var target, out var mi))
            {
                slider.onValueChanged.AddListener(f => mi.Invoke(target, new object[] { f }));
            }
            else
            {
                Debug.LogWarning($"[{name}] 找不到方法 {methodName}(float) 於 View 或 ViewModel。");
            }
        }

        private void IndexVmObservables()
        {
            _vmObservableMap.Clear();

            var flags   = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var members = _viewModel.GetType().GetMembers(flags);

            foreach (var m in members)
            {
                Type opType = null; Type valueType = null; Func<object> getter = null;

                switch (m)
                {
                    case FieldInfo fi:
                        if (TryGetObservableInfo(fi.FieldType, out opType, out valueType))
                            getter = () => fi.GetValue(_viewModel);
                        break;
                    case PropertyInfo pi:
                        if (pi.CanRead && TryGetObservableInfo(pi.PropertyType, out opType, out valueType))
                            getter = () => pi.GetValue(_viewModel);
                        break;
                }
                if (getter == null) continue;

                var opInstance = getter();
                if (opInstance == null) continue;

                var baseName    = DeriveBaseName(m.Name);
                var valueProp   = opType.GetProperty("Value");
                var forceNotify = opType.GetMethod("ForceNotify");

                if (valueProp == null) continue;

                _vmObservableMap[baseName] = new ObservableBinding
                {
                    OpInstance  = opInstance,
                    ValueType   = valueType,
                    ValueProp   = valueProp,
                    ForceNotify = forceNotify
                };
            }
        }

        /// <summary>
        /// 透過反射自動掃描並註冊所有 Button 欄位。
        /// </summary>
        private void AutoRegisterComponents()
        {
            var fields = GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var field in fields)
            {
                // 只處理有 [SerializeField] 屬性的欄位
                bool hasSerializedAttr = field.GetCustomAttribute(typeof(SerializeField)) != null;
                if (!hasSerializedAttr)
                {
                    continue;
                }

                // 取綁定基名：優先 [BindName]，否則由欄位名推導
                string baseName = field.GetCustomAttribute<BindNameAttribute>()?.Name ?? DeriveBaseName(field.Name);
                object obj      = field.GetValue(this);

                if (obj == null) { Debug.LogWarning($"[AutoRegisterComponents] 欄位 {field.Name} 為 null。"); continue; }

                // 找 ViewModel 內是否有對應的 ObservableProperty（名稱以 baseName 配對）
                _vmObservableMap.TryGetValue(baseName, out var bind);

                // 判斷型別是否為 Button
                if (obj is InputField tf)
                {
                    // 若 VM 有對應屬性，UI→VM：string
                    if (bind != null && bind.ValueType == typeof(string))
                    {
                        tf.onValueChanged.AddListener(s =>
                        {
                            SetVmObservableValue(bind, s);
                        });
                    }
                    // 沒對應就不綁 UI→VM（可視需要警告）
                }
                else if (obj is Toggle toggle)
                {
                    if (bind != null && bind.ValueType == typeof(bool))
                    {
                        toggle.onValueChanged.AddListener(v =>
                        {
                            SetVmObservableValue(bind, v);
                        });
                    }
                }
                else if (obj is Slider slider)
                {
                    if (bind != null && IsNumeric(bind.ValueType))
                    {
                        slider.onValueChanged.AddListener(f =>
                        {
                            // 轉換到對應數值型別
                            object boxed = ConvertToType(f, bind.ValueType);
                            SetVmObservableValue(bind, boxed);
                        });
                    }
                }
                else if (obj is Button)
                {
                    Button btn = (Button)obj;

                    if (btn != null)
                    {
                        string method = $"On{baseName}Click";
                        TryBindButton(btn, method);
                    }
                    else
                    {
                        Debug.LogWarning($"[AutoRegisterComponents] 欄位 {field.Name} 為 null，請確認是否有綁定。");
                    }
                }                
            }
        }

        private static object ConvertToType(float f, Type t)
        {
            if (t == typeof(float)) return f;
            if (t == typeof(double)) return (double)f;
            if (t == typeof(int)) return (int)f;
            if (t == typeof(uint)) return (uint)f;
            if (t == typeof(short)) return (short)f;
            if (t == typeof(ushort)) return (ushort)f;
            if (t == typeof(long)) return (long)f;
            if (t == typeof(ulong)) return (ulong)f;
            if (t == typeof(decimal)) return (decimal)f;
            // 其他數值型別需求再補
            return f;
        }

        private static void SetVmObservableValue(ObservableBinding bind, object value)
        {
            try
            {
                // 直接寫入 ObservableProperty<T>.Value
                bind.ValueProp.SetValue(bind.OpInstance, value);
                // 如需強制通知（相同值也推），可視需求呼叫：
                // bind.ForceNotify?.Invoke(bind.OpInstance, null);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[ViewBase] 寫入 VM Observable 失敗：{e}");
            }
        }

        private static string DeriveBaseName(string fieldName)
        {
            // 去前綴
            string s = fieldName;
            if (s.StartsWith("m_")) s = s.Substring(2);
            if (s.StartsWith("_")) s = s.Substring(1);

            // 轉為 PascalCase（保留原順序，僅首字母大寫）
            if (s.Length > 0) s = char.ToUpperInvariant(s[0]) + (s.Length > 1 ? s.Substring(1) : "");

            // 移除常見尾綴
            s = StripSuffix(s, "Button", "Btn");
            s = StripSuffix(s, "Toggle", "Tgl");
            s = StripSuffix(s, "Slider", "Sld");
            s = StripSuffix(s, "InputField", "Input", "Field", "Txt", "Text");

            return s;
        }

        private static string StripSuffix(string s, params string[] suffixes)
        {
            foreach (var suf in suffixes)
            {
                if (s.EndsWith(suf, StringComparison.Ordinal))
                    return s.Substring(0, s.Length - suf.Length);
            }
            return s;
        }

        // 依「方法名 + 參數型別」在 View → ViewModel 兩邊依序查找
        private bool TryResolveMethod(string methodName, Type[] argTypes, out object target, out MethodInfo mi)
        {
            const BindingFlags bf = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            mi = _viewModel.GetType().GetMethod(methodName, bf, binder: null, types: argTypes, modifiers: null);
            if (mi != null)
            {
                target = _viewModel;
                return true;
            }

            target  = null;
            mi      = null;
            return false;
        }

        /****************************************************
         * 依照ViewModel的ObservableProperty 去綁定Compoents
         * **************************************************/
        /// <summary>
        /// 新增：只綁定 ViewModel 中的 ObservableProperty 成員 → 對應到 View 的 UI 欄位
        /// </summary>
        private void AutoBindObservables()
        {
            // 取 View 內所有 [SerializeField] 的 UI 欄位，建立名稱→物件對照
            var viewUiMap       = BuildSerializedUiMap();

            // 找 ViewModel 內所有 ObservableProperty<T> 欄位 + 屬性
            BindingFlags flags  = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            MemberInfo[] mis    = _viewModel.GetType().GetMembers(flags);

            foreach (var m in mis)
            {
                Type opType         = null;         // ObservableProperty< T >
                Type valueType      = null;         // T
                Func<object> getter = null;         // 取得成員值
                object opInstance   = null;         // ObservableProperty實體

                switch (m)
                {
                    case FieldInfo fi:
                        if (TryGetObservableInfo(fi.FieldType, out opType, out valueType))
                            getter = () => fi.GetValue(_viewModel);
                        break;
                    case PropertyInfo pi:
                        if (pi.CanRead && TryGetObservableInfo(pi.PropertyType, out opType, out valueType))
                            getter = () => pi.GetValue(_viewModel);
                        break;
                }

                // 取出 ObservableProperty<T> 實體
                if (getter == null) continue;

                opInstance = getter();
                if (opInstance == null) continue;

                // 名稱推導（移除前綴，去尾綴）
                string baseName = DeriveBaseName(m.Name);

                // 找對應 UI 欄位
                if (!viewUiMap.TryGetValue(baseName, out var uiObj))
                {
                    // 找不到就跳過（可視需要開 Debug.Log）
                    continue;
                }

                // 建立 setter 與訂閱 OnValueChanged
                var subscribeMi     = opType.GetMethod("Subscribe");   // IDisposable Subscribe(Action<T>)
                var forceNotifyMi   = opType.GetMethod("ForceNotify");
                var valueProp       = opType.GetProperty("Value");

                if (subscribeMi == null || forceNotifyMi == null || valueProp == null) continue;

                // 依 UI 型別與 T 決定如何設定
                var disposer = BindOneObservableToUi(opInstance, valueType, uiObj);
                if (disposer != null)
                {
                    _disposables.Add(disposer);
                }

                // 推一次初值
                forceNotifyMi?.Invoke(opInstance, null);
            }
        }

        private Dictionary<string, object> BuildSerializedUiMap()
        {
            var map     = new Dictionary<string, object>(StringComparer.Ordinal);
            var fields  = GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var f in fields)
            {
                if (f.GetCustomAttribute(typeof(SerializeField)) == null) continue;

                var comp = f.GetValue(this);
                if (comp == null) continue;

                if (comp is Button || comp is Toggle || comp is InputField || comp is Slider || comp is Text || comp is RawImage || comp is Image)
                {
                    var key     = DeriveBaseName(f.Name);
                    map[key]    = comp;
                }
            }
            return map;
        }

        private static bool TryGetObservableInfo(Type t, out Type opType, out Type valueType)
        {
            opType = null; valueType = null;

            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(ObservableProperty<>))
            {
                opType      = t;
                valueType   = t.GetGenericArguments()[0];
                return true;
            }

            return false;
        }

        private IDisposable BindOneObservableToUi(object opInstance, Type valueType, object uiObj)
        {
            // 動態建立 Action<T>，把值推到 UI
            if (uiObj is InputField input && valueType == typeof(string))
            {
                Action<string> setter = v => { if (input != null) input.SetTextWithoutNotify(v ?? string.Empty); };
                return Subscribe(opInstance, setter);
            }
            else if (uiObj is Text textUi) // 純顯示
            {
                Action<object> setter = v => { if (textUi != null) textUi.text = v?.ToString() ?? string.Empty; };
                return SubscribeObject(opInstance, valueType, setter);
            }
            else if (uiObj is Toggle toggle && valueType == typeof(bool))
            {
                Action<bool> setter = v => { if (toggle != null) toggle.SetIsOnWithoutNotify(v); };
                return Subscribe(opInstance, setter);
            }
            else if (uiObj is Slider slider && IsNumeric(valueType))
            {
                Action<object> setter = v =>
                {
                    if (slider == null) return;
                    float f = Convert.ToSingle(v);
                    // Approximately 防止slider抖動
                    if (!Mathf.Approximately(slider.value, f)) slider.SetValueWithoutNotify(f);
                };
                return SubscribeObject(opInstance, valueType, setter);
            }
            else if (uiObj is Image img)
            {
                // Image ← Sprite（最推薦）
                if (valueType == typeof(Sprite))
                {
                    Action<Sprite> setter = sp =>
                    {
                        if (img == null) return;
                        img.sprite = sp;
                        // 如想讓尺寸跟著圖走，可在這裡加：img.SetNativeSize();
                    };
                    return Subscribe(opInstance, setter);
                }
                // Image ← Texture2D（自動轉 Sprite）
                else if (typeof(Texture2D).IsAssignableFrom(valueType))
                {
                    Action<object> setter = v =>
                    {
                        if (img == null) return;
                        var tex = v as Texture2D;
                        img.sprite = tex != null ? GetOrCreateSprite(tex) : null;
                    };
                    return SubscribeObject(opInstance, valueType, setter);
                }
                else if (valueType == typeof(string))
                {
                    Action<string> setter = v =>
                    {
                        if (img == null) return;

                        ImageUtils.LoadImageFromUrl(img, v);
                    };
                    return Subscribe(opInstance, setter);
                }
            }
            else if (uiObj is RawImage raw)
            {
                // RawImage ← Texture（含 Texture2D / RenderTexture / WebCamTexture）
                if (typeof(Texture).IsAssignableFrom(valueType))
                {
                    Action<object> setter = v =>
                    {
                        if (raw == null) return;
                        raw.texture = v as Texture; // null OK，等於清空
                    };
                    return SubscribeObject(opInstance, valueType, setter);
                }
                // （可選）RawImage ← Sprite（取 sprite.texture）
                else if (valueType == typeof(Sprite))
                {
                    Action<Sprite> setter = sp =>
                    {
                        if (raw == null) return;
                        raw.texture = sp != null ? sp.texture : null;
                    };
                    return Subscribe(opInstance, setter);
                }
                else if (valueType == typeof(string))
                {
                    Action<string> setter = v =>
                    {
                        if (raw == null) return;

                        ImageUtils.LoadImageFromUrl(raw, v);
                    };
                    return Subscribe(opInstance, setter);
                }
            }

            // 型別不相容就不綁
            return null;
        }
        private static bool IsNumeric(Type t)
        {
            return t == typeof(float) || t == typeof(double) || t == typeof(int) ||
                   t == typeof(uint) || t == typeof(short) || t == typeof(ushort) ||
                   t == typeof(long) || t == typeof(ulong) || t == typeof(decimal);
        }

        // 使用 ObservableProperty<T>.Subscribe(Action<T>)
        private static IDisposable Subscribe<T>(object opInstance, Action<T> handler)
        {
            var mi = opInstance.GetType().GetMethod("Subscribe");
            return (IDisposable)mi.Invoke(opInstance, new object[] { handler });
        }

        // 當 T 不是編譯期已知時，透過反射把 object 轉型
        private static IDisposable SubscribeObject(object opInstance, Type valueType, Action<object> handler)
        {
            var mi          = opInstance.GetType().GetMethod("Subscribe");
            // 建立 Action<T> 動態委派
            var actionType  = typeof(Action<>).MakeGenericType(valueType);
            var del         = Delegate.CreateDelegate(actionType, handler.Target, handler.Method); // handler(object) 也能接到 T，因為 .NET 允許實參協變

            return (IDisposable)mi.Invoke(opInstance, new object[] { del });
        }

        /****************************************
         * 實作IUIView
         * **************************************/        
        public int SortIdx { get; set; }
        public void RefreshLanguage()
        {

        }

        /*********************************************************************************
        * 綁定 ViewModel 的 *{BaseName}Visible* 與 *Visible*（根）到對應的 UI 物件／本體。
        *********************************************************************************/
        private void AutoBindVisibility()
        {
            // 1) 建 UI 映射：欄位→物件（包含 GameObject 與常見 UI 元件）
            Dictionary<string, GameObject> viewUiMap = BuildSerializedUiMapForVisibility();

            // 2) 針對每個 UI 欄位：找 {BaseName}Visible → bool
            foreach (var kv in viewUiMap)
            {
                string baseName     = kv.Key;
                GameObject targetGO = kv.Value;             // 統一是 GameObject
                string visibleKey   = baseName + "Visible"; // 例：Abc + Visible → abcVisible

                if (_vmObservableMap.TryGetValue(visibleKey, out var bind) && bind.ValueType == typeof(bool))
                {
                    // 訂閱 VM→UI：切換此欄位對應的 GO
                    var disp = Subscribe<bool>(bind.OpInstance, v => ToggleUI(targetGO, v));
                    _disposables.Add(disp);

                    // 初值推送
                    bind.ForceNotify?.Invoke(bind.OpInstance, null);
                }
            }

            // 3) 根物件（View 本體）可綁 Visible
            if (_vmObservableMap.TryGetValue("UiVisible", out var rootBind) && rootBind.ValueType == typeof(bool))
            {
                var rootGO  = this.gameObject;
                var disp    = Subscribe<bool>(rootBind.OpInstance, v => ToggleUI(rootGO, v));
                _disposables.Add(disp);

                rootBind.ForceNotify?.Invoke(rootBind.OpInstance, null);
            }
        }

        // 供 Visible 綁定使用：把 [SerializeField] 欄位整理成 BaseName→GameObject。
        // 支援：GameObject、Button、Toggle、InputField、Slider、Text（需要時可自行擴充）        
        private Dictionary<string, GameObject> BuildSerializedUiMapForVisibility()
        {
            var map     = new Dictionary<string, GameObject>(StringComparer.Ordinal);
            var fields  = GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var f in fields)
            {
                // 只處理有 [BindVisibleTarget] 的欄位
                var attr = f.GetCustomAttribute<BindVisibleTargetAttribute>();
                if (attr == null) continue;

                var obj = f.GetValue(this);
                if (obj == null) continue;

                GameObject go = null;

                if (obj is GameObject goField)
                {
                    go = goField;
                }
                else if (obj is Component comp)
                {
                    go = comp != null ? comp.gameObject : null;
                }

                if (go == null) continue;

                // 取 key：優先屬性覆寫，否則用 DeriveBaseName
                var key     = string.IsNullOrEmpty(attr.Name)
                            ? DeriveBaseName(f.Name)
                            : attr.Name;
                map[key]    = go;                    // Abc → 該欄位所在 GO
            }

            return map;
        }

        /***************************************
		 * UI Visible
		 * *************************************/
        private void ToggleUI(GameObject ui, bool bEnabled)
        {
            // 狀態一致 不需要改變
            if (ui.activeSelf == bEnabled)
            {
                return;
            }

            FadeBase[] fadeList = ui.GetComponents<FadeBase>();

            if (fadeList == null || fadeList.Length == 0)
            {
                ui.SetActive(bEnabled);
                return;
            }

            if (bEnabled)
            {
                ui.SetActive(true);

                Array.ForEach<FadeBase>(fadeList, (fadeComp) =>
                {
                    if (fadeComp == null)
                    {
                        return;
                    }

                    fadeComp.PleaseStartYourPerformance(true, null);
                });
            }
            else
            {
                int finishCounter = 0;

                Array.ForEach<FadeBase>(fadeList, (fadeComp) =>
                {
                    if (fadeComp == null)
                    {
                        return;
                    }

                    fadeComp.PleaseStartYourPerformance(false, () =>
                    {
                        if (++finishCounter == fadeList.Length)
                        {
                            ui.SetActive(false);
                        }
                    });
                });
            }
        }

        /***************************************
        * Sprite 快取，避免重複從 Texture2D 產生
        * *************************************/
        private readonly Dictionary<Texture2D, Sprite> _spriteFromTexCache = new();

        private static Sprite ToSprite(Texture2D tex)
        {
            if (tex == null) return null;
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
        }

        private Sprite GetOrCreateSprite(Texture2D tex)
        {
            if (tex == null) return null;
            if (_spriteFromTexCache.TryGetValue(tex, out var sp) && sp != null) return sp;
            sp                          = ToSprite(tex);
            _spriteFromTexCache[tex]    = sp;
            return sp;
        }
    }
}
