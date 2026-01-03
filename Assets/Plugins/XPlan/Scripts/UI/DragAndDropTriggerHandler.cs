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
using UnityEngine;
using UnityEngine.EventSystems;

namespace XPlan.UI
{
    public class DragAndDropTriggerHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public Action<PointerEventData> beginDragDelegate, dragingDelgate, endDragDelegate;

        public void OnBeginDrag(PointerEventData eventData)
        {
            beginDragDelegate?.Invoke(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            dragingDelgate?.Invoke(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            endDragDelegate?.Invoke(eventData);
        }
    }
}