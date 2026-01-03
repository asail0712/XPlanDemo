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
using XPlan.Interface;

namespace XPlan.UI
{
    public class ButtonToCommandAdapter
    {
        private readonly ICommand command;

        public ButtonToCommandAdapter(IButton button, ICommand command)
        {
            this.command = command;
            button.OnClick += OnClickButtonHandler;
        }

        private void OnClickButtonHandler()
        {
            command.Execute();
        }
    }
}