using System;

namespace XPlan.Interface
{
    public interface IButton
    {
        event Action OnClick;
    }
}