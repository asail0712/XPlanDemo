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
using UnityEngine;

namespace XPlan.Components
{
    // 將Content居中
    public class ContentRealignment : MonoBehaviour
    {
        private void Awake()
        {
            RectTransform parentRect    = transform.parent.GetComponent<RectTransform>();
            RectTransform currentRect   = GetComponent<RectTransform>();

            if (parentRect == null || currentRect == null)
            {
                return;
            }

			float widthOffset               = currentRect.rect.width - parentRect.rect.width;
            Vector2 anchorPos               = currentRect.anchoredPosition;
            anchorPos.x                     = (widthOffset > 0) ? widthOffset / 2 : 0;
            currentRect.anchoredPosition    = anchorPos;
        }
    }
}