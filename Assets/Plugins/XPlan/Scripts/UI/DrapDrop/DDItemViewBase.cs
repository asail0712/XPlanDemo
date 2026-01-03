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
using UnityEngine.EventSystems;

namespace XPlan.UI
{
    public class DDItemViewBase<TItemViewModel> : ItemViewBase<TItemViewModel>
        , IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerEnterHandler, IPointerExitHandler
        where TItemViewModel : DDItemViewModelBase
    {
        // 快取好的委派（避免每次事件都反射 Invoke）
        private Action<TItemViewModel, PointerEventData> _onBeginDrag;
        private Action<TItemViewModel, PointerEventData> _onDrag;
        private Action<TItemViewModel, PointerEventData> _onEndDrag;
        private Action<TItemViewModel, PointerEventData> _onDrop;

        private Action<TItemViewModel, PointerEventData> _onDragEnter;
        private Action<TItemViewModel, PointerEventData> _onDragExit;

        // ===============================
        // Event Trigger
        // ===============================
        public void OnBeginDrag(PointerEventData eventData)
        {
            _onBeginDrag?.Invoke(_viewModel, eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            _onDrag?.Invoke(_viewModel, eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _onEndDrag?.Invoke(_viewModel, eventData);
        }

        public void OnDrop(PointerEventData eventData)
        {
            _onDrop?.Invoke(_viewModel, eventData);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _onDragEnter?.Invoke(_viewModel, eventData);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _onDragExit?.Invoke(_viewModel, eventData);
        }
    }
}
