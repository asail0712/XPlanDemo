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

namespace XPlan.Command
{
    public class CommandSelector : ICommand
    {
        private readonly ICondition condition;
        private readonly ICommand trueConditionCommand;
        private readonly ICommand falseConditionCommand;

        public CommandSelector(ICondition condition, ICommand trueConditionCommand, ICommand falseConditionCommand)
        {
            this.condition              = condition;
            this.trueConditionCommand   = trueConditionCommand;
            this.falseConditionCommand  = falseConditionCommand;
        }

        public void Execute()
        {
            condition.Evaluate(ConditionHandler);
        }

        private void ConditionHandler(bool value)
        {
            if (value)
            {
                trueConditionCommand?.Execute();
            }
            else
            {
                falseConditionCommand?.Execute();
            }
        }
    }
}