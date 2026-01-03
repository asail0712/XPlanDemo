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
using XPlan.Net;

namespace XPlan.Net
{
    public interface IWebRequestPolicy
    {
        /// <summary>
        /// 若回傳 true，代表已攔截並完成 callback，不需再送實際 WebRequest
        /// </summary>
        bool TryHandle(WebRequestBase req, Action<ApiResult<WebResponseData>> callback);
    }
}