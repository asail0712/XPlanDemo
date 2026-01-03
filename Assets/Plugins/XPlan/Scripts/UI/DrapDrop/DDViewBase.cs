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
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;

namespace XPlan.UI
{
    public class DDViewBase<TDDViewModel, TDDItemView, TDDItemViewModel> : TableViewBase<TDDViewModel, TDDItemView, TDDItemViewModel>
        where TDDViewModel : DDViewModelBase<TDDItemViewModel>
        where TDDItemView : DDItemViewBase<TDDItemViewModel>, new()
        where TDDItemViewModel : DDItemViewModelBase, new()
    {
        private static readonly BindingFlags MethodFlags    = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private static readonly BindingFlags FieldFlags     = BindingFlags.Instance | BindingFlags.NonPublic;

        protected override void OnTableViewReady(TDDViewModel vm)
        {
            TDDItemView[] viewList = _activeItemViews.Values.ToArray();

            foreach(TDDItemView itemView in viewList)
            {
                AutoBindDragEvents(vm, itemView);
            }            
        }

        private void AutoBindDragEvents(TDDViewModel vm, TDDItemView itemView)
        {
            var vmType      = typeof(TDDViewModel);
            var itemVMType  = typeof(TDDItemViewModel);

            // 找出 ViewModel 上所有標了 DragBinding 的方法
            var methods = vmType.GetMethods(MethodFlags)
                .Select(m => new
                {
                    Method = m,
                    Attr = m.GetCustomAttribute<DragBindingAttribute>()
                })
                .Where(x => x.Attr != null)
                .ToList();

            if (methods.Count == 0)
                return;

            foreach (var entry in methods)
            {
                var method  = entry.Method;
                var phase   = entry.Attr.Phase;

                // 檢查方法簽名
                var ps = method.GetParameters();
                if (ps.Length != 2 ||
                    ps[0].ParameterType != itemVMType ||
                    ps[1].ParameterType != typeof(PointerEventData))
                {
                    continue;
                }

                // 建立 delegate
                var del = (Action<TDDItemViewModel, PointerEventData>)
                    Delegate.CreateDelegate(
                        typeof(Action<TDDItemViewModel, PointerEventData>),
                        vm,   // ★ TableViewBase 裡的 ViewModel
                        method);

                // 依 DragPhase 指派到 ItemView 的對應欄位
                AssignToItemView(itemView, phase, del);
            }
        }

        private static void AssignToItemView(
        TDDItemView itemView,
        DragPhase phase,
        Action<TDDItemViewModel, PointerEventData> del)
        {
            string fieldName = phase switch
            {
                DragPhase.Begin => "_onBeginDrag",
                DragPhase.Drag => "_onDrag",
                DragPhase.End => "_onEndDrag",
                DragPhase.Drop => "_onDrop",
                DragPhase.DragEnter => "_onDragEnter",
                DragPhase.DragExit => "_onDragExit",
                _ => null
            };

            if (fieldName == null)
                return;

            var field = typeof(DDItemViewBase<TDDItemViewModel>).GetField(fieldName, FieldFlags);

            if (field == null)
            {
                Debug.LogError(
                    $"[DragBinding] 找不到欄位 {fieldName} 於 {typeof(TDDItemView).Name}");
                return;
            }

            field.SetValue(itemView, del);
        }
    }
}
