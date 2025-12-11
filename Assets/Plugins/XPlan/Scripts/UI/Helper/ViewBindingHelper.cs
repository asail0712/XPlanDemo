using System;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    /// <summary>
    /// ViewBase 專用的綁定 Helper：
    /// - VM Observable 索引
    /// - UI → VM 綁定
    /// - VM → UI 綁定
    /// - Visible 綁定
    /// - Sprite 快取
    /// </summary>
    internal static class ViewBindingHelper
    {
        #region ==== VM Observable 索引 ====

        public static void IndexVmObservables(
            ViewModelBase viewModel,
            Dictionary<string, ObservableBinding> map)
        {
            map.Clear();

            var flags   = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var members = viewModel.GetType().GetMembers(flags);

            foreach (var m in members)
            {
                Type opType         = null;
                Type valueType      = null;
                Func<object> getter = null;

                switch (m)
                {
                    case FieldInfo fi:
                        if (TryGetObservableInfo(fi.FieldType, out opType, out valueType))
                            getter = () => fi.GetValue(viewModel);
                        break;

                    case PropertyInfo pi:
                        if (pi.CanRead && TryGetObservableInfo(pi.PropertyType, out opType, out valueType))
                            getter = () => pi.GetValue(viewModel);
                        break;
                }

                if (getter == null) continue;

                var opInstance = getter();
                if (opInstance == null) continue;

                var baseName    = DeriveBaseName(m.Name);
                var valueProp   = opType.GetProperty("Value");
                var forceNotify = opType.GetMethod("ForceNotify");

                if (valueProp == null) continue;

                map[baseName]   = new ObservableBinding
                {
                    OpInstance  = opInstance,
                    ValueType   = valueType,
                    ValueProp   = valueProp,
                    ForceNotify = forceNotify
                };
            }
        }

        #endregion

        #region ==== UI → VM 綁定 (AutoRegisterComponents) ====

        public static void AutoRegisterComponents(
            MonoBehaviour view,
            Dictionary<string, ObservableBinding> vmObservableMap)
        {
            var fields = view.GetType().GetFields(
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var field in fields)
            {
                // 僅處理有 [SerializeField] 的欄位（不動程式碼產生 / 動態欄位）
                bool hasSerializedAttr = field.GetCustomAttribute(typeof(SerializeField)) != null;
                if (!hasSerializedAttr)
                    continue;

                string baseName =
                    field.GetCustomAttribute<BindNameAttribute>()?.Name
                    ?? DeriveBaseName(field.Name);

                object obj = field.GetValue(view);
                if (obj == null)
                {
                    Debug.LogWarning($"[AutoRegisterComponents] 欄位 {field.Name} 為 null。");
                    continue;
                }

                vmObservableMap.TryGetValue(baseName, out var bind);

                if (obj is InputField tf)
                {
                    if (bind != null && bind.ValueType == typeof(string))
                    {
                        tf.onValueChanged.AddListener(s =>
                        {
                            SetVmObservableValue(bind, s);
                        });
                    }
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
                            object boxed = ConvertToType(f, bind.ValueType);
                            SetVmObservableValue(bind, boxed);
                        });
                    }
                }
                else if (obj is ScrollRect scrollRect)
                {
                    // ScrollRect 的 normalizedPosition 是 Vector2
                    if (bind != null && bind.ValueType == typeof(float))
                    {
                        // 訂閱 onValueChanged 事件。當捲動位置改變時觸發
                        scrollRect.onValueChanged.AddListener(v2 =>
                        {                            
                            SetVmObservableValue(bind, v2);
                        });
                    }
                }
                // Button 的 MVVM 點擊綁定改用 IL Weaving 做，這裡就不處理了
            }
        }

        private static void SetVmObservableValue(ObservableBinding bind, object value)
        {
            try
            {
                bind.ValueProp.SetValue(bind.OpInstance, value);
                // 如果要「同值也通知」就打開：
                // bind.ForceNotify?.Invoke(bind.OpInstance, null);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[ViewBindingHelper] 寫入 VM Observable 失敗：{e}");
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
            return f;
        }

        private static bool IsNumeric(Type t)
        {
            return t == typeof(float) || t == typeof(double) || t == typeof(int) ||
                   t == typeof(uint) || t == typeof(short) || t == typeof(ushort) ||
                   t == typeof(long) || t == typeof(ulong) || t == typeof(decimal);
        }

        #endregion

        #region ==== VM → UI 綁定 (AutoBindObservables) ====

        public static void AutoBindObservables(
            MonoBehaviour view,
            ViewModelBase viewModel,
            List<IDisposable> disposables,
            SpriteCache spriteCache,
            Action anythingChange = null)
        {
            var viewUiMap   = BuildSerializedUiMap(view);

            var flags       = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var mis         = viewModel.GetType().GetMembers(flags);

            foreach (var m in mis)
            {
                Type opType         = null;
                Type valueType      = null;
                Func<object> getter = null;

                switch (m)
                {
                    case FieldInfo fi:
                        if (TryGetObservableInfo(fi.FieldType, out opType, out valueType))
                            getter = () => fi.GetValue(viewModel);
                        break;

                    case PropertyInfo pi:
                        if (pi.CanRead && TryGetObservableInfo(pi.PropertyType, out opType, out valueType))
                            getter = () => pi.GetValue(viewModel);
                        break;
                }

                if (getter == null) continue;

                var opInstance = getter();
                if (opInstance == null) continue;

                string baseName = DeriveBaseName(m.Name);
                if (!viewUiMap.TryGetValue(baseName, out var uiObj))
                    continue;

                var disp = BindOneObservableToUi(opInstance, valueType, uiObj, spriteCache, anythingChange);
                if (disp == null)
                    continue;

                disposables.Add(disp);

                // 推一次初值
                var forceNotifyMi = opType.GetMethod("ForceNotify");
                forceNotifyMi?.Invoke(opInstance, null);
            }
        }

        private static Dictionary<string, object> BuildSerializedUiMap(MonoBehaviour view)
        {
            var map     = new Dictionary<string, object>(StringComparer.Ordinal);
            var fields  = view.GetType().GetFields(
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var f in fields)
            {
                if (f.GetCustomAttribute(typeof(SerializeField)) == null)
                    continue;

                var comp = f.GetValue(view);
                if (comp == null) continue;

                if (comp is Button || comp is Toggle || comp is InputField ||
                    comp is Slider || comp is Text || comp is RawImage || comp is Image ||
                    comp is TextMeshProUGUI)
                {
                    var key = DeriveBaseName(f.Name);
                    map[key] = comp;
                }
            }

            return map;
        }

        private static IDisposable BindOneObservableToUi(
            object opInstance,
            Type valueType,
            object uiObj,
            SpriteCache spriteCache,
            Action anythingChange = null)
        {
            // InputField ⇐ string
            if (uiObj is InputField input && valueType == typeof(string))
            {
                Action<string> setter = v =>
                {
                    if (input == null) return;
                    input.SetTextWithoutNotify(v ?? string.Empty);

                    anythingChange?.Invoke();
                };
                return Subscribe(opInstance, setter);
            }
            // Text ⇐ any (ToString)
            if (uiObj is Text textUi)
            {
                Action<object> setter = v =>
                {
                    if (textUi == null) return;
                    textUi.text = v?.ToString() ?? string.Empty;

                    anythingChange?.Invoke();
                };
                return SubscribeObject(opInstance, valueType, setter);
            }
            // TMP ⇐ any (ToString)
            if (uiObj is TextMeshProUGUI tmpUi)
            {
                Action<object> setter = v =>
                {
                    if (tmpUi == null) return;
                    tmpUi.text = v?.ToString() ?? string.Empty;

                    anythingChange?.Invoke();
                };
                return SubscribeObject(opInstance, valueType, setter);
            }
            // Toggle ⇐ bool
            if (uiObj is Toggle toggle && valueType == typeof(bool))
            {
                Action<bool> setter = v =>
                {
                    if (toggle == null) return;
                    toggle.SetIsOnWithoutNotify(v);

                    anythingChange?.Invoke();
                };
                return Subscribe(opInstance, setter);
            }
            // Slider ⇐ numeric
            if (uiObj is Slider slider && IsNumeric(valueType))
            {
                Action<object> setter = v =>
                {
                    if (slider == null) return;
                    float f = Convert.ToSingle(v);
                    if (!Mathf.Approximately(slider.value, f))
                    {
                        slider.SetValueWithoutNotify(f);
                        anythingChange?.Invoke();
                    }
                };
                return SubscribeObject(opInstance, valueType, setter);
            }
            // Image ⇐ Sprite / Texture2D / string(url)
            if (uiObj is Image img)
            {
                if (valueType == typeof(Sprite))
                {
                    Action<Sprite> setter = sp =>
                    {
                        if (img == null) return;
                        img.sprite = sp;
                        anythingChange?.Invoke();
                    };
                    return Subscribe(opInstance, setter);
                }

                if (typeof(Texture2D).IsAssignableFrom(valueType))
                {
                    Action<object> setter = v =>
                    {
                        if (img == null) return;
                        var tex     = v as Texture2D;
                        img.sprite  = tex != null
                            ? spriteCache.GetOrCreateSprite(tex)
                            : null;
                        anythingChange?.Invoke();
                    };
                    return SubscribeObject(opInstance, valueType, setter);
                }

                if (valueType == typeof(string))
                {
                    Action<string> setter = url =>
                    {
                        if (img == null) return;
                        ImageUtils.LoadImageFromUrl(img, url, (dummy) => 
                        {
                            anythingChange?.Invoke();
                        });                        
                    };
                    return Subscribe(opInstance, setter);
                }
            }
            // RawImage ⇐ Texture / Sprite / string(url)
            if (uiObj is RawImage raw)
            {
                if (typeof(Texture).IsAssignableFrom(valueType))
                {
                    Action<object> setter = v =>
                    {
                        if (raw == null) return;
                        raw.texture = v as Texture;
                        anythingChange?.Invoke();
                    };
                    return SubscribeObject(opInstance, valueType, setter);
                }

                if (valueType == typeof(Sprite))
                {
                    Action<Sprite> setter = sp =>
                    {
                        if (raw == null) return;
                        raw.texture = sp?.texture;
                        anythingChange?.Invoke();
                    };
                    return Subscribe(opInstance, setter);
                }

                if (valueType == typeof(string))
                {
                    Action<string> setter = url =>
                    {
                        if (raw == null) return;
                        ImageUtils.LoadImageFromUrl(raw, url, (dummy) => 
                        {
                            anythingChange?.Invoke();
                        }); 
                    };
                    return Subscribe(opInstance, setter);
                }
            }
            if (uiObj is ScrollRect scrollRect && valueType == typeof(Vector2))
            {
                // 訂閱 VM Observable 的變化
                Action<Vector2> setter = v =>
                {
                    if (scrollRect == null) return;

                    // 檢查是否接近當前值，避免不必要的設定和浮點數問題
                    // 使用 Vector2.Approximately 來比較兩個 Vector2 是否接近
                    if (!Vector2.Equals(scrollRect.normalizedPosition, v))
                    {
                        // 將 VM 傳來的 Vector2 值設定給 normalizedPosition (包含 X 和 Y 軸)
                        scrollRect.normalizedPosition = v;
                        anythingChange?.Invoke();
                    }
                };
                // 假設 Subscribe 方法接受 Vector2 泛型和 Action<Vector2>
                return Subscribe(opInstance, setter);
            }

            return null;
        }

        private static IDisposable Subscribe<T>(object opInstance, Action<T> handler)
        {
            var mi = opInstance.GetType().GetMethod("Subscribe");
            return (IDisposable)mi.Invoke(opInstance, new object[] { handler });
        }

        private static IDisposable SubscribeObject(
            object opInstance,
            Type valueType,
            Action<object> handler)
        {
            var mi          = opInstance.GetType().GetMethod("Subscribe");
            var actionType  = typeof(Action<>).MakeGenericType(valueType);
            var del         = Delegate.CreateDelegate(actionType, handler.Target, handler.Method);
            return (IDisposable)mi.Invoke(opInstance, new object[] { del });
        }

        #endregion

        #region ==== VM Observable → View 方法綁定 (OnXXXXChange) ====

        /// <summary>
        /// 掃描 View 上所有有 [ObBinding] 的方法，
        /// 自動幫它們綁到 ViewModel 的 ObservableProperty 上。
        ///
        /// 規則：
        /// - 先找方法名對應的 Observable：
        ///   1) 先用 DeriveBaseName(methodName) 找
        ///   2) 若找不到，且 methodName 形如 OnXXXXChange，則取 XXXX 再 DeriveBaseName
        /// - 方法必須只有一個參數
        /// - 方法參數型別要與 ObservableProperty<T> 的 T 一致
        /// - 成功則 Subscribe，並把 IDisposable 丟進 disposables
        /// - 綁定後會呼叫一次 ForceNotify() 推當前值
        /// </summary>
        public static void AutoBindObservableHandlers(
            MonoBehaviour view,
            Dictionary<string, ObservableBinding> vmObservableMap,
            List<IDisposable> disposables)
        {
            if (view == null) return;

            var flags   = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var methods = view.GetType().GetMethods(flags);

            foreach (var mi in methods)
            {
                // 1) 只處理有 [ObBinding] 的方法
                if (mi.GetCustomAttribute<ObBindingAttribute>() == null)
                    continue;

                // 2) 檢查參數：必須只有一個
                var ps = mi.GetParameters();
                if (ps.Length != 1)
                {
                    Debug.LogError(
                        $"[AutoBindObservableHandlers] 方法 {mi.DeclaringType?.Name}.{mi.Name} 必須有且只有一個參數。");
                    continue;
                }

                var paramType           = ps[0].ParameterType;

                // 3) 依命名規則推導對應的 Observable 名稱
                //    先直接用方法名稱 DeriveBaseName
                ObservableBinding bind  = null;
                string key              = DeriveBaseName(mi.Name);

                if (!vmObservableMap.TryGetValue(key, out bind))
                {
                    // 若找不到，且是 OnXXXXChange 這種命名，就取中間的 XXXX
                    const string prefix = "On";
                    const string suffix = "Change";

                    var name = mi.Name;
                    if (name.StartsWith(prefix, StringComparison.Ordinal) &&
                        name.EndsWith(suffix, StringComparison.Ordinal) &&
                        name.Length > (prefix.Length + suffix.Length))
                    {
                        var mid = name.Substring(prefix.Length, name.Length - prefix.Length - suffix.Length);
                        key     = DeriveBaseName(mid);
                        vmObservableMap.TryGetValue(key, out bind);
                    }
                }

                if (bind == null)
                {
                    Debug.LogError(
                        $"[AutoBindObservableHandlers] 找不到與方法 {mi.DeclaringType?.Name}.{mi.Name} 對應的 ViewModel Observable（Key: {key}）。");
                    continue;
                }

                // 4) 檢查型別是否一致
                if (bind.ValueType != paramType)
                {
                    Debug.LogError(
                        $"[AutoBindObservableHandlers] 方法 {mi.DeclaringType?.Name}.{mi.Name} 的參數型別為 {paramType.Name}，" +
                        $"但對應的 ObservableProperty<{bind.ValueType.Name}> 不相容。");
                    continue;
                }

                // 5) 建立 Action<T> delegate
                Delegate del;
                try
                {
                    var handlerType = typeof(Action<>).MakeGenericType(paramType);
                    del             = Delegate.CreateDelegate(handlerType, view, mi);
                }
                catch (Exception e)
                {
                    Debug.LogError(
                        $"[AutoBindObservableHandlers] 建立委派失敗：{mi.DeclaringType?.Name}.{mi.Name}，Exception: {e}");
                    continue;
                }

                // 6) 使用 ObservableProperty<T>.Subscribe(Action<T>) 做綁定
                try
                {
                    var subscribeMi = bind.OpInstance.GetType().GetMethod("Subscribe");
                    if (subscribeMi == null)
                    {
                        Debug.LogError(
                            $"[AutoBindObservableHandlers] Observable '{key}' 找不到 Subscribe 方法。");
                        continue;
                    }

                    var disp = (IDisposable)subscribeMi.Invoke(bind.OpInstance, new object[] { del });
                    if (disp != null)
                        disposables.Add(disp);

                    // 7) 綁完後推一次初值
                    bind.ForceNotify?.Invoke(bind.OpInstance, null);
                }
                catch (Exception e)
                {
                    Debug.LogError(
                        $"[AutoBindObservableHandlers] 訂閱 Observable '{key}' 失敗，方法 {mi.DeclaringType?.Name}.{mi.Name}，Exception: {e}");
                }
            }
        }

        #endregion

        #region ==== Visible 綁定 ====

        public static void AutoBindVisibility(
            MonoBehaviour view,
            Dictionary<string, ObservableBinding> vmObservableMap,
            List<IDisposable> disposables)
        {
            var uiMap = BuildSerializedUiMapForVisibility(view);

            // 1) 單一元件的 {BaseName}Visible
            foreach (var kv in uiMap)
            {
                string baseName     = kv.Key;
                GameObject targetGO = kv.Value;
                string visibleKey   = baseName + "Visible";

                if (vmObservableMap.TryGetValue(visibleKey, out var bind) &&
                    bind.ValueType == typeof(bool))
                {
                    var disp = Subscribe<bool>(bind.OpInstance, v => ViewVisibilityHelper.ToggleUI(targetGO, v));
                    disposables.Add(disp);
                    bind.ForceNotify?.Invoke(bind.OpInstance, null);
                }
            }

            // 2) 整個 View 的 {view.name}Visible
            // 直接使用view的name 會有clone字樣
            string viewClassName    = view.GetType().Name;
            string rootVisibleKey   = viewClassName + "Visible";

            if (vmObservableMap.TryGetValue(rootVisibleKey, out var rootBind) &&
                rootBind.ValueType == typeof(bool))
            {
                var rootGO  = view.gameObject;
                var disp    = Subscribe<bool>(rootBind.OpInstance, v => ViewVisibilityHelper.ToggleUI(rootGO, v));
                disposables.Add(disp);
                rootBind.ForceNotify?.Invoke(rootBind.OpInstance, null);
            }
        }

        private static Dictionary<string, GameObject> BuildSerializedUiMapForVisibility(
            MonoBehaviour view)
        {
            var map     = new Dictionary<string, GameObject>(StringComparer.Ordinal);
            var fields  = view.GetType().GetFields(
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var f in fields)
            {
                var attr = f.GetCustomAttribute<BindVisibleTargetAttribute>();
                if (attr == null) continue;

                var obj = f.GetValue(view);
                if (obj == null) continue;

                GameObject go = obj switch
                {
                    GameObject goField => goField,
                    Component comp => comp != null ? comp.gameObject : null,
                    _ => null
                };

                if (go == null) continue;

                var key = string.IsNullOrEmpty(attr.Name)
                        ? DeriveBaseName(f.Name)
                        : attr.Name;

                map[key] = go;
            }

            return map;
        }

        #endregion

        #region ==== Observable 共用工具 ====

        /// <summary>
        /// 檢查型別是否為 ObservableProperty&lt;T&gt;，若是則回傳 opType 與 T。
        /// View / ViewModel 都可以共用這個方法。
        /// </summary>
        public static bool TryGetObservableInfo(Type t, out Type opType, out Type valueType)
        {
            opType      = null;
            valueType   = null;

            if (t != null &&
                t.IsGenericType &&
                t.GetGenericTypeDefinition() == typeof(ObservableProperty<>))
            {
                opType      = t;
                valueType   = t.GetGenericArguments()[0];
                return true;
            }

            return false;
        }

        #endregion

        #region ==== 共用字串處理 / Sprite 快取 ====

        public static string DeriveBaseName(string fieldName)
        {
            string s = fieldName;

            if (s.StartsWith("m_")) s = s.Substring(2);
            if (s.StartsWith("_")) s = s.Substring(1);

            if (s.Length > 0)
                s = char.ToUpperInvariant(s[0]) + (s.Length > 1 ? s.Substring(1) : "");

            s = StripSuffix(s, "Button", "Btn");
            s = StripSuffix(s, "Toggle", "Tgl");
            s = StripSuffix(s, "Slider", "Sld");
            s = StripSuffix(s, "Scroll");
            s = StripSuffix(s, "InputField", "Input", "Field", "Txt", "Text");
            s = StripSuffix(s, "Image", "Img");

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

        #endregion
    }

    /// <summary>
    /// 每個 View 自己持有一份 SpriteCache，
    /// 幫忙管理 Texture2D → Sprite 的快取與釋放。
    /// </summary>
    internal sealed class SpriteCache : IDisposable
    {
        private readonly Dictionary<Texture2D, Sprite> _cache = new();

        public Sprite GetOrCreateSprite(Texture2D tex)
        {
            if (tex == null) return null;

            if (_cache.TryGetValue(tex, out var sp) && sp != null)
                return sp;

            sp = Sprite.Create(
                tex,
                new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f),
                100f);

            _cache[tex] = sp;
            return sp;
        }

        public void Dispose()
        {
            foreach (var kv in _cache)
            {
                if (kv.Value != null)
                    GameObject.Destroy(kv.Value);
            }
            _cache.Clear();
        }
    }
}
