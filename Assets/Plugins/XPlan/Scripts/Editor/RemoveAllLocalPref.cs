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
using UnityEditor;
using UnityEngine;

using XPlan.Utility;

namespace XPlan.Editors
{
    public class RemoveAllLocalPrefs : MonoBehaviour
    {
        [MenuItem("XPlanTools/Remove All LocalPrefs")]
        private static void RemoveLocalPref()
        {
            PlayerPrefs.DeleteAll();
        }
    }
}