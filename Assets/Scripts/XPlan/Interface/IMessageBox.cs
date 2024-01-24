using System;

namespace XPlan.Interface
{
    public interface IMessageBox
    {
        void Execute(Action ok, Action cancel);
    }
}