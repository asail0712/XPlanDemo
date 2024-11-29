using System;

namespace XPlan.Net
{
    public interface IEventHandler
    {
        void Open(IEventHandler eventHandler);
        void Close(IEventHandler eventHandler, bool bErrorHappen);
        void Error(IEventHandler eventHandler, string errorTxt);
        void Message(IEventHandler eventHandler, string msgTxt);
    }
}
