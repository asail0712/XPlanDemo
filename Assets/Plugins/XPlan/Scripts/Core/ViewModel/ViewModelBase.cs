// ==============================================================================
// XPlan Framework
//
// Copyright (c) 2026 Asail
// All rights reserved.
//
// Author  : Asail0712
// Project : XPlan
// Description:
//     A modular framework for Unity projects, focusing on MVVM architecture,
//     runtime tooling, event-driven design, and extensibility.
//
// Contact : asail0712@gmail.com
// GitHub  : https://github.com/asail0712/XPlanDemo
//
// Unauthorized copying, modification, or distribution of this file,
// via any medium, is strictly prohibited without prior permission.
// ==============================================================================
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

using XPlan.UI;
using XPlan.Utility;

namespace XPlan
{
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

        // 原始的無參數建構子 (用於一般 ViewModel)
        public ViewModelBase() : this(true)
        {
            // 呼叫新的帶參數建構子，並傳入 true
        }

        // 新增的帶參數建構子 (用於 ItemViewModelBase 這種需要控制註冊的子類)
        protected ViewModelBase(bool bRegister)
        {
            // 根據參數決定是否註冊
            if (bRegister)
            {
                VMLocator.Register(this);
            }

            EnableAutoNotifyForObservables();
        }

        protected override void OnDispose(bool bAppQuit)
        {
            foreach (var d in _autoChangeSubs)
            {
                d?.Dispose();
            }

            _autoChangeSubs.Clear();

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
                        if (ViewBindingHelper.TryGetObservableInfo(fi.FieldType, out opType, out valueType))
                            getter = () => fi.GetValue(this);
                        break;
                    case PropertyInfo pi:
                        if (pi.CanRead && ViewBindingHelper.TryGetObservableInfo(pi.PropertyType, out opType, out valueType))
                            getter = () => pi.GetValue(this);
                        break;
                }

                if (getter == null) continue;

                var opInstance = getter();
                if (opInstance == null) continue;

                var baseName    = ViewBindingHelper.DeriveBaseName(m.Name);
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
    }
}