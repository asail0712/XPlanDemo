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

namespace XPlan
{
    // 標記此欄位可與ViewModel成員繫結（預設由欄位名推導）
    // 名稱為 BindName
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class BindNameAttribute : Attribute
    {
        public string Name { get; }
        public BindNameAttribute(string name) => Name = name;
    }
}
