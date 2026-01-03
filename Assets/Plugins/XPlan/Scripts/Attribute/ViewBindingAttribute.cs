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

namespace XPlan
{
    /// <summary>
    /// 標記：這個 ViewModel Method 要被當成「按鈕點擊」處理。
    /// 預設用方法名稱推導 Button 欄位名稱，例如：
    ///   OnDemoTriggerClick → demoTriggerBtn
    /// 如果不想用命名規則，也可以在建構子給 ButtonName。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public sealed class ViewBindingAttribute : Attribute
    {
        public ViewBindingAttribute()
        {
        }
    }
}