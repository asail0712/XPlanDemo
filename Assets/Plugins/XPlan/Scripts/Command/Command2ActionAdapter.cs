using System;

using XPlan.Interface;

namespace XPlan.Command
{
    public class Command2ActionAdapter : ICommand
    {
        private readonly Action commandAction;

        public Command2ActionAdapter(Action commandAction)
        {
            this.commandAction = commandAction;
        }

        public void Execute()
        {
            commandAction.Invoke();
        }
    }
}