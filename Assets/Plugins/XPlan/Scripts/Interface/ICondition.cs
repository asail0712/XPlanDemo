using System;

namespace XPlan.Interface
{
    public interface ICondition
    {
        void Evaluate(Action<bool> resultHandler);
    }
}