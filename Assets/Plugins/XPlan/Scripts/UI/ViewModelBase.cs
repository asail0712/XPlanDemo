using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using XPlan.Utility;

namespace XPlan.UI
{
    public static class VMLocator
    {
        private static readonly Dictionary<Type, object> _map                           = new();
        private static readonly Dictionary<Type, TaskCompletionSource<object>> _waiters = new();

        public static void Register(ViewModelBase vm)
        {
            var t   = vm.GetType();
            _map[t] = vm; // 後註冊者覆蓋
        }

        public static bool TryGet<T>(out T vm) where T : ViewModelBase
        {
            if (_map.TryGetValue(typeof(T), out var obj))
            {
                vm = obj as T;
                return vm != null;
            }
            vm = null;
            return false;
        }

        public static T Get<T>() where T : ViewModelBase
        {
            return _map[typeof(T)] as T;
        }

        public static void GetOrWaitAsync<T>(float waitTime, Action<T> finishAction) where T : ViewModelBase
        {
            if (TryGet<T>(out T vm))
            {
                finishAction?.Invoke(vm);
            }

            MonoBehaviourHelper.StartCoroutine(GetOrWaitAsync_Imp<T>(waitTime, finishAction));
        }

        private static IEnumerator GetOrWaitAsync_Imp<T>(float waitTime, Action<T> finishAction) where T : ViewModelBase
        {
            // 允許在下一個 frame 再檢查（讓 VM 有機會在同一幀的其他 Awake/OnEnable 完成註冊）
            yield return null;

            float elapsed = 0f;
            bool infinite = waitTime <= 0f;

            // 輪詢直到找到或超時
            while (infinite || elapsed < waitTime)
            {
                if (TryGet<T>(out var vm) && vm != null)
                {
                    finishAction?.Invoke(vm);
                    yield break;
                }

                elapsed += Time.unscaledDeltaTime; // 不受 TimeScale 影響
                yield return null;
            }

            // 超時（可視需求移除或改成回傳 null）
            Debug.LogWarning($"[VMLocator] 等待 {typeof(T).Name} 超時（{waitTime:F2}s）。");
        }

        public static void Unregister(ViewModelBase vm)
        {
            var t = vm.GetType();
            if (_map.TryGetValue(t, out var obj) && ReferenceEquals(obj, vm))
            {
                _map.Remove(t);
            }
        }

        public static void Clear() => _map.Clear();
    }

    public class ObservableProperty<T>
    {
        private T _value;
        public T Value
        {
            get => _value;
            set
            {
                if (Equals(_value, value)) return;
                _value = value;
                OnValueChanged?.Invoke(_value);
            }
        }

        public event Action<T> OnValueChanged;

        public ObservableProperty(T defaultValue = default)
        {
            _value = defaultValue;
        }

        /// 僅設值，不觸發事件（初始化或批量更新時好用）
        public void SetSilently(T v) => _value = v;

        /// 強制發送一次當前值（例如 UI 初次同步）
        public void ForceNotify()
        {
            OnValueChanged?.Invoke(_value);
        }

        /// 方便訂閱並取得 IDisposable 解除訂閱器
        public IDisposable Subscribe(Action<T> handler)
        {
            OnValueChanged += handler;
            return new Disposer(() => OnValueChanged -= handler);
        }

        private sealed class Disposer : IDisposable
        {
            private Action _dispose;
            public Disposer(Action dispose) => _dispose = dispose;
            public void Dispose() { _dispose?.Invoke(); _dispose = null; }
        }
    }

    public class ViewModelBase : LogicComponent
    {
        private readonly List<IDisposable> _autoChangeSubs = new(); // 自動 On{Name}Change 訂閱

        public ViewModelBase()
        {
            VMLocator.Register(this);

            EnableAutoNotifyForObservables();
        }

        protected override void OnDispose(bool bAppQuit)
        {
            VMLocator.Unregister(this);
        }

        /*******************************************************************************
        * 在子類別建構子「最後」呼叫一次，會掃描當前 VM 內所有 ObservableProperty{T}，
        * 並自動綁定到 On{Name}Change(T)。
        *******************************************************************************/
        protected void EnableAutoNotifyForObservables()
        {
            // 先解除舊的（避免重複呼叫導致多重訂閱）
            foreach (var d in _autoChangeSubs)
            {
                d?.Dispose();
            }
            _autoChangeSubs.Clear();

            // 取出所有成員資訊
            BindingFlags flags      = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            MemberInfo[] members    = GetType().GetMembers(flags);

            foreach (var m in members)
            {
                Type opType         = null;
                Type valueType      = null;
                Func<object> getter = null;

                switch (m)
                {
                    case FieldInfo fi:
                        if (TryGetObservableInfo(fi.FieldType, out opType, out valueType))
                            getter = () => fi.GetValue(this);
                        break;
                    case PropertyInfo pi:
                        if (pi.CanRead && TryGetObservableInfo(pi.PropertyType, out opType, out valueType))
                            getter = () => pi.GetValue(this);
                        break;
                }

                if (getter == null) continue;

                var opInstance = getter();
                if (opInstance == null) continue;

                var baseName    = DeriveBaseName(m.Name);
                var methodName  = $"On{baseName}Change";

                // 找 On{Name}Change(T)（允許 private/protected）
                var mi = GetType().GetMethod(methodName, flags, binder: null, types: new[] { valueType }, modifiers: null);
                if (mi == null) continue;

                // 建立 Action<T>：v => mi.Invoke(this, new object[]{v})
                var actionType  = typeof(Action<>).MakeGenericType(valueType);
                var handler     = Delegate.CreateDelegate(actionType, this, mi, false);

                if (handler == null)
                {
                    // 綁不到委派（例如參數型別不符），跳過
                    continue;
                }

                // 呼叫 ObservableProperty<T>.Subscribe(Action<T>)
                var subscribeMi = opType.GetMethod("Subscribe", BindingFlags.Instance | BindingFlags.Public);
                if (subscribeMi == null) continue;

                var disposable = (IDisposable)subscribeMi.Invoke(opInstance, new object[] { handler });
                if (disposable != null) _autoChangeSubs.Add(disposable);
            }
        }

        protected static bool TryGetObservableInfo(Type t, out Type opType, out Type valueType)
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

        protected static string DeriveBaseName(string memberName)
        {
            // 去前綴
            string s = memberName;
            if (s.StartsWith("m_")) s = s.Substring(2);
            if (s.StartsWith("_"))  s = s.Substring(1);

            // 首字大寫
            if (s.Length > 0) s = char.ToUpperInvariant(s[0]) + (s.Length > 1 ? s.Substring(1) : "");

            // 常見尾綴移除（可依你專案再擴充）
            s = StripSuffix(s, "Property", "Prop", "Field");
            return s;
        }

        protected static string StripSuffix(string s, params string[] suffixes)
        {
            foreach (var suf in suffixes)
                if (s.EndsWith(suf, StringComparison.Ordinal))
                    return s.Substring(0, s.Length - suf.Length);
            return s;
        }
    }
}