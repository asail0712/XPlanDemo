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
using Unity.Collections;
using UnityEditor;

// 參考
// https://blog.csdn.net/weixin_42565127/article/details/125990221

namespace XPlan.Editors
{
    public static class LeakDetectionControl
    {
        [MenuItem("XPlanTools/Leak Detection/Enable")]
        private static void LeakDetection()
		{
            NativeLeakDetection.Mode = NativeLeakDetectionMode.Enabled;
		}

        [MenuItem("XPlanTools/Leak Detection/With Stack Trace")]
        private static void LeakDetectionWithStackTrace()
        {
            NativeLeakDetection.Mode = NativeLeakDetectionMode.EnabledWithStackTrace;
        }

        [MenuItem("XPlanTools/Leak Detection/Disable")]
        private static void NoLeakDetection()
        {
            NativeLeakDetection.Mode = NativeLeakDetectionMode.Disabled;
        }
    }   
}