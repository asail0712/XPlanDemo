using System;

namespace XPlan
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class LogAspectAttribute : Attribute
    {
        public Type LoggerType { get; }

        public LogAspectAttribute(Type loggerType)
        {
            LoggerType = loggerType;
        }
    }
}
