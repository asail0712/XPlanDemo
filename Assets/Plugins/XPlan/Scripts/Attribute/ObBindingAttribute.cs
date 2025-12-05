using System;

namespace XPlan
{
    /// <summary>
    /// 標記：這個 函數 要被綁定在ViewModel上的某個Observable
    ///   OnHpChange(int) → int hp
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class ObBindingAttribute : Attribute
    {
        public ObBindingAttribute()
        {
        }
    }
}

