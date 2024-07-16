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