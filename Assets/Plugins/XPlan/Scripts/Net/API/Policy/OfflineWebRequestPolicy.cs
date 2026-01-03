// ==============================================================================
// XPlan Framework
//
// Copyright (c) 2026 Asail
// All rights reserved.
//
// Author  : Asail0712
// Project : XPlan
// Description:
//     A modular framework for Unity projects, focusing on MVVM architecture,
//     runtime tooling, event-driven design, and extensibility.
//
// Contact : asail0712@gmail.com
// GitHub  : https://github.com/asail0712/XPlanDemo
//
// Unauthorized copying, modification, or distribution of this file,
// via any medium, is strictly prohibited without prior permission.
// ==============================================================================
using System;

namespace XPlan.Net
{
    /// <summary>
    /// Offline 模式策略：
    /// - Mode = Offline 時攔截所有 WebRequestBase，直接回傳成功 (可接 mock 產生器)
    /// - Mode = Online 時不攔截，交回正常 UnityWebRequest 流程
    /// </summary>
    public sealed class OfflineWebRequestPolicy : IWebRequestPolicy
    {
        /// <summary>
        /// 可以把產生 payload 的邏輯塞在這個 delegate。
        /// 若為 null，則使用預設 payload（"true"）
        /// </summary>
        private readonly Func<WebRequestBase, WebResponseData> payloadFactory;

        public OfflineWebRequestPolicy(Func<WebRequestBase, WebResponseData> payloadFactory = null)
        {
            this.payloadFactory = payloadFactory;
        }

        public bool TryHandle(WebRequestBase request, Action<ApiResult<WebResponseData>> callback)
        {
            // 只有 Offline 才攔截
            if (WebRequestConfig.Mode != WebRequestMode.Offline)
                return false;

            // callback 允許為 null（但通常你會傳）
            if (callback == null)
                return true; // 已攔截，但沒人要結果

            WebResponseData payload;

            try
            {
                // 有提供 mock/工廠就用；沒有就用預設
                payload = payloadFactory != null ? payloadFactory.Invoke(request)
                                                 : CreateDefaultPayload(request);
            }
            catch (Exception e)
            {
                // Offline 產生 payload 失敗，就回 Fail
                callback.Invoke(ApiResult<WebResponseData>.Fail(
                    $"[OfflineWebRequestPolicy] Mock payload factory exception: {e}"
                ));
                return true;
            }

            callback.Invoke(ApiResult<WebResponseData>.Success(payload));
            return true;
        }

        private static WebResponseData CreateDefaultPayload(WebRequestBase request)
        {
            // 預設回 "true" 當作最簡成功訊號（ContentType 用 json 方便上層處理）
            return new WebResponseData(
                "application/json",
                "",
                null
            );
        }
    }
}