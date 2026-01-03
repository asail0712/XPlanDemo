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
using XPlan;

namespace XPlan
{
    public static class VMLocator
    {
        private static readonly Dictionary<Type, object> _map = new();

        // 等待 VM 的 callback pool（改成可取消）
        private static readonly Dictionary<Type, List<(int id, Action<ViewModelBase> cb)>> _waiters = new();
        private static int _waitSeq = 0;

        // ★ 新增：VM 被解除註冊事件
        public static event Action<Type, ViewModelBase> VMUnregistered;

        public static void Register(ViewModelBase vm)
        {
            var t = vm.GetType();
            _map[t] = vm;

            if (_waiters.TryGetValue(t, out var list))
            {
                _waiters.Remove(t);
                foreach (var (_, cb) in list)
                    cb?.Invoke(vm);
            }
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

        /// <summary>
        /// ★ 改：回傳 token 可取消等待（View Destroy 時用）
        /// </summary>
        public static IDisposable GetOrWait<T>(Action<T> finishAction) where T : ViewModelBase
        {
            if (TryGet<T>(out T vm))
            {
                finishAction?.Invoke(vm);
                return EmptyDisposable.Instance;
            }

            var t = typeof(T);
            if (!_waiters.TryGetValue(t, out var list))
                _waiters[t] = list = new();

            var id = ++_waitSeq;
            list.Add((id, vmObj => finishAction?.Invoke(vmObj as T)));

            return new Disposer(() =>
            {
                if (_waiters.TryGetValue(t, out var l))
                {
                    l.RemoveAll(x => x.id == id);
                    if (l.Count == 0) _waiters.Remove(t);
                }
            });
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