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
using System.Reflection;

namespace XPlan.Weaver.Runtime
{
    /// <summary>
    /// 給 IL Weaving 的 Hook 入口：
    /// 在衍生類別中產生一個 Hook 方法
    /// 就會被這裡自動呼叫
    /// </summary>
    public static class WeaverHookInvoker
    {
        public static void Invoke(object target, string hookName)
        {
            if (target == null) return;

            var type    = target.GetType();
            var method  = type.GetMethod(
                            hookName,
                            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            if (method == null)
                return;

            if (method.ReturnType != typeof(void))
                return;

            if (method.GetParameters().Length != 0)
                return;

            try
            {
                method.Invoke(target, null);
            }
            catch (TargetInvocationException tie)
            {
                UnityEngine.Debug.LogError($"[WeaverHookInvoker] Hook throw. type={type.FullName}, hook={hookName}\n{tie.InnerException}");
                throw; // 你想要不中斷就拿掉 throw，但先抓到錯比較重要
            }
        }
    }
}