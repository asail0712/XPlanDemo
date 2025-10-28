using System;

namespace XPlan.Net
{
    public interface IEventHandler
    {
        void Open(IConnectHandler eventHandler);
        void Close(IConnectHandler eventHandler, bool bErrorHappen);
        void Error(IConnectHandler eventHandler, string errorTxt);
        void Message(IConnectHandler eventHandler, string msgTxt);
        void Binary(IConnectHandler eventHandler, byte[] byteArr);
    }
}
