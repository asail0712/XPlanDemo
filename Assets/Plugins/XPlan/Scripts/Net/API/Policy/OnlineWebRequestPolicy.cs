using System;

namespace XPlan.Net
{
    public sealed class OnlineWebRequestPolicy : IWebRequestPolicy
    {
        public bool TryHandle(WebRequestBase request, Action<ApiResult<WebResponseData>> callback)
        {
            // 永遠不攔截，交回 WebRequestBase 正常流程
            return false;
        }
    }
}