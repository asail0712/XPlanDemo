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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XPlan.UI
{
    public class CanvasInfo : MonoBehaviour
    {
        [HideInInspector]
        public int defaultDisplayIdx;

        private void Awake()
        {
            Canvas canvas       = GetComponent<Canvas>();
            defaultDisplayIdx   = canvas.targetDisplay;
        }
    }
}
