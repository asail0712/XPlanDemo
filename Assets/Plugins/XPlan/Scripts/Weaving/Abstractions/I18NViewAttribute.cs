using System;

namespace XPlan
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class I18NViewAttribute : Attribute
    {
        public string Key { get; }

        public I18NViewAttribute(string key)
        {
            Key = key;
        }
    }
}