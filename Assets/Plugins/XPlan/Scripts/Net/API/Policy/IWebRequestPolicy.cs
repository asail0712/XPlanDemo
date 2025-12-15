using System;
using UnityEngine;
using XPlan.Net;

namespace XPlan.Net
{
    public interface IWebRequestPolicy
    {
        /// <summary>
        /// 若回傳 true，代表已攔截並完成 callback，不需再送實際 WebRequest
        /// </summary>
        bool TryHandle(WebRequestBase req, Action<ApiResult<WebResponseData>> callback);
    }
}