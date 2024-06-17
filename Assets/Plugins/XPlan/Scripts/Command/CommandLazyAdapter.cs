using System;

using XPlan.Interface;

namespace XPlan.Command
{
    public class CommandLazyAdapter : Lazy<ICommand>, ICommand
    {
        public CommandLazyAdapter(Func<ICommand> valueFactory) : base(valueFactory)
        {
        }

        public void Execute()
        {
            Value.Execute();
        }
    }
}