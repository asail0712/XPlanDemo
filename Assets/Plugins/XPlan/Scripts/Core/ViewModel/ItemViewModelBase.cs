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
namespace XPlan
{
    // 作為列表中單一項目 ViewModel 的基底。
    // 繼承 ViewModelBase 以便使用 ObservableProperty 的自動通知機制。
    public class ItemViewModelBase : ViewModelBase
    {
        // 可以新增一些通用的 Item 屬性，例如：
        // public ObservableProperty<bool> IsSelected { get; } = new();

        // **重要：ItemViewModelBase 不應呼叫 VMLocator.Register(this)**
        public ItemViewModelBase()
            : base(false)
        {
            
        }
    }
}