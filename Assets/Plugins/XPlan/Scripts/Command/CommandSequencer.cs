using XPlan.Interface;

namespace XPlan.Command
{
    public class CommandSequencer : ICommand
    {
        private readonly ICommand[] commands;

        public CommandSequencer(params ICommand[] commands)
        {
            this.commands = commands;
        }

        public void Execute()
        {
            foreach (var command in commands)
            {
                command.Execute();
            }
        }
    }
}