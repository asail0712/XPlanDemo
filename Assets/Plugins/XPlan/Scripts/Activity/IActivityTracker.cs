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

namespace XPlan.Activity
{
    public interface IActivityTracker<TInfo> : IActivityTracker
    {
        event Action<TInfo> OnFeatureTouched;
    }


    // XPlan 只關心 IActivityTracker 不關心 delegate傳遞的資料型態
    public interface IActivityTracker
    {
        void Touch(string feature);
        void Tick();
        void Flush(bool bForce = false);
    }
}