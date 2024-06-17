using System;

namespace XPlan.Interface
{
    public interface ISwitcher
    {
        Action<int> OnChoose { get; set; }
    }
}