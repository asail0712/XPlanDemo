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
            // 掃描是否帶有 TrackerAttribute 的方法            
            if (!HasAnyTrackedMethodInGame())
                return;

            MonoBehaviourHelper.StartCoroutine(TickTracker());
        }

        private static IEnumerator TickTracker()
        {
            while (true)
            {                
                if (Activity.Current != null)
                    Activity.Current.Tick();

                yield return new WaitForSeconds(10f);
            }
        }

        private static bool HasAnyTrackedMethodInGame()
        {
            Assembly gameAsm;
            try
            {
                gameAsm = Assembly.Load("Assembly-CSharp");
            }
            catch
            {
                return false;
            }

            Type[] types;
            try
            {
                types = gameAsm.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                types = e.Types.Where(t => t != null).ToArray();
            }

            foreach (var type in types)
            {
                var methods = type.GetMethods(
                    BindingFlags.Instance |
                    BindingFlags.Static |
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.DeclaredOnly
                );

                foreach (var method in methods)
                {
                    if (method.IsAbstract) continue;
                    if (method.IsDefined(typeof(TrackerAttribute), inherit: false))
                        return true; // 找到就停
                }
            }

            return false;
        }
    }
}