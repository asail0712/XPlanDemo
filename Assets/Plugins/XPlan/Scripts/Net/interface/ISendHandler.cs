using System;

namespace XPlan.Net
{
    public interface ISendHandler
    {
        bool Send(string mess);
        bool Send(byte[] bytes);
    }
}
