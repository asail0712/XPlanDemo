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
    /// 預設用方法名稱推導 文字輸入 欄位名稱，例如：
    ///   OnSpeakChange → speakTxt
    /// 如果不想用命名規則，也可以在建構子給 tfName。
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class InputTfBindingAttribute : Attribute
    {
        /// <summary>
        /// 對應 View 上的 Button 欄位名稱（可選）
        /// </summary>
        public string TfName { get; }

        public InputTfBindingAttribute()
        {
        }

        public InputTfBindingAttribute(string tfName)
        {
            TfName = tfName;
        }
    }
}