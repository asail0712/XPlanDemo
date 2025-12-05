using System;

namespace XPlan
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class NotifyHandlerAttribute : Attribute
    {
        public NotifyHandlerAttribute()
        {
        }
    }
}