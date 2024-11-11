using System;

namespace XPlan.Net
{
    public interface IConnectHandler
    {
        Uri Url { get; }
        void Connect();
        void Reconnect();
        void CloseConnect();
    }
}
