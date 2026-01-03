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


namespace XPlan.Components
{
    public class FaceToCamera : MonoBehaviour
    {
        private void LateUpdate()
        {
            if(Camera.main == null)
			{
                return;
			}

            // 使物體始終面向相機
            transform.LookAt(Camera.main.transform);
            // 使物體保持正面朝向相機
            transform.rotation = Quaternion.Euler(0.0f, transform.rotation.eulerAngles.y, 0.0f);
        }
    }
}