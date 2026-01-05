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
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XPlan;

namespace XPlan
{
    public static class VMLocator
    {
        private static readonly Dictionary<Type, object> _map = new();

        // 等待 VM 的 callback pool（改成可取消）
        private static readonly Dictionary<Type, List<(GameObject, Action<ViewModelBase>)>> _waiters = new();

        // ★ 新增：VM 被解除註冊事件
        public static event Action<Type, ViewModelBase> VMUnregistered;

        public static void Register(ViewModelBase vm)
        {
            var t   = vm.GetType();
            _map[t] = vm;

            if (_waiters.TryGetValue(t, out var list))
            {
                // 找機會 清掉已經死亡的 go
                list.RemoveAll(w => w.Item1 == null);

                foreach (var (_, cb) in list)
                    cb?.Invoke(vm);
            }
        }
        public static void Unregister(ViewModelBase vm)
        {
            var t = vm.GetType();
            if (_map.TryGetValue(t, out var obj) && ReferenceEquals(obj, vm))
            {
                _map.Remove(t);
                VMUnregistered?.Invoke(t, vm); // ★ 通知還活著的 View 回到 Wait
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static void GetOrWait<T>(GameObject go, Action<T> finishAction) where T : ViewModelBase
        {   
            // 先註冊
            var t = typeof(T);
            if (!_waiters.TryGetValue(t, out var list))
            {
                _waiters[t] = list = new();
            }
            else
            {    // 避免同一個 GameObject 重複註冊
                if (list.Any(w => w.Item1 == go))
                    return;
            }

            list.Add((go, vmObj => finishAction?.Invoke(vmObj as T)));

            // 再嘗試取得
            if (TryGet<T>(out T vm))
            {
                finishAction?.Invoke(vm);
            }
        }

        public static void CancelWait<T>(GameObject go) where T : ViewModelBase
        {
            var t = typeof(T);
            if (_waiters.TryGetValue(t, out var list))
            {
                list.RemoveAll(w => w.Item1 == go);
                if (list.Count == 0)
                {
                    _waiters.Remove(t);
                }
            }
        }

        private static bool TryGet<T>(out T vm) where T : ViewModelBase
        {
            if (_map.TryGetValue(typeof(T), out var obj))
            {
                vm = obj as T;
                return vm != null;
            }

            vm = null;
            return false;
        }

        private sealed class Disposer : IDisposable
        {
            private Action _dispose;
            public Disposer(Action dispose) => _dispose = dispose;
            public void Dispose() { _dispose?.Invoke(); _dispose = null; }
        }

        private sealed class EmptyDisposable : IDisposable
        {
            public static readonly EmptyDisposable Instance = new();
            public void Dispose() { }
        }
    }
}