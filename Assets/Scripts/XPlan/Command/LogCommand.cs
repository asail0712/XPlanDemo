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