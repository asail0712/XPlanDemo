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

using XPlan.Interface;

namespace XPlan.Command
{
    public class LogCommand : ICommand
    {
        private readonly Func<string> logMessageFactory;

        public LogCommand(string logMessage)
        {
            logMessageFactory = () => logMessage;
        }

        public LogCommand(Func<string> logMessageFactory)
        {
            this.logMessageFactory = logMessageFactory;
        }

        public void Execute()
        {
            Debug.Log(logMessageFactory.Invoke());
        }
    }
}