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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using XPlan.Utility;

namespace XPlan.Activity
{
    public static class ActivityTrackingInstaller
    {
        public static void InstallIfNeeded()
        {
            // 1️⃣ 掃描所有帶有 TrackerAttribute 的方法
            List<MethodInfo> trackedMethods = ScanTrackedMethods();

            if (trackedMethods.Count == 0)
                return;

            MonoBehaviourHelper.StartCoroutine(TickTracker());
        }

        private static IEnumerator TickTracker()
        {
            while (true)
            {                
                if (Activity.Current != null)
                    Activity.Current.Tick();

                yield return new WaitForSeconds(3f);
            }
        }

        private static List<MethodInfo> ScanTrackedMethods()
        {
            var result              = new List<MethodInfo>();
            Assembly[] assemblies   = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var asm in assemblies)
            {
                // 避免掃 Unity / System 的組件（效能 + 安全）
                if (asm.FullName.StartsWith("System") ||
                    asm.FullName.StartsWith("Unity") ||
                    asm.FullName.StartsWith("mscorlib"))
                    continue;

                Type[] types;
                try
                {
                    types = asm.GetTypes();
                }
                catch (ReflectionTypeLoadException e)
                {
                    types = e.Types.Where(t => t != null).ToArray();
                }

                foreach (var type in types)
                {
                    MethodInfo[] methods = type.GetMethods(
                        BindingFlags.Instance |
                        BindingFlags.Static |
                        BindingFlags.Public |
                        BindingFlags.NonPublic
                    );

                    foreach (var method in methods)
                    {
                        if (method.IsAbstract)
                            continue;

                        if (method.GetCustomAttribute<TrackerAttribute>(inherit: true) != null)
                        {
                            result.Add(method);
                        }
                    }
                }
            }

            return result;
        }
    }
}