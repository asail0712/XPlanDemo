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

namespace XPlan.Net
{
    public enum WebRequestMode { Online, Offline }

    public static class WebRequestConfig
    {
        private static WebRequestMode mode = WebRequestMode.Online;
        
        public static IWebRequestPolicy OnlinePolicy { get; set; }  = new OnlineWebRequestPolicy();
        public static IWebRequestPolicy OfflinePolicy { get; set; } = new OfflineWebRequestPolicy();

        public static WebRequestMode Mode
        {
            get => mode;
            set
            {
                if (mode == value)
                    return;

                mode = value;
                ApplyPolicy(mode);
            }
        }

        static WebRequestConfig()
        {
            // 啟動時套一次預設
            WebRequestPolicyProvider.Policy = OnlinePolicy;
        }

        private static void ApplyPolicy(WebRequestMode mode)
        {
            // switch的回傳值 表達式
            WebRequestPolicyProvider.Policy = mode switch
            {
                WebRequestMode.Online   => OnlinePolicy,
                WebRequestMode.Offline  => OfflinePolicy,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}
