using System;

namespace XPlan.Net
{
    public enum WebRequestMode { Online, Offline }

    public static class WebRequestConfig
    {
        private static WebRequestMode mode = WebRequestMode.Online;
        
        public static IWebRequestPolicy OnlinePolicy { get; set; }  = new OnlineWebRequestPolicy();
        public static IWebRequestPolicy OfflinePolicy { get; set; } = new OfflineWebRequestPolicy();

        public static WebRequestMode Mode
        {
            get => mode;
            set
            {
                if (mode == value)
                    return;

                mode = value;
                ApplyPolicy(mode);
            }
        }

        static WebRequestConfig()
        {
            // 啟動時套一次預設
            WebRequestPolicyProvider.Policy = OnlinePolicy;
        }

        private static void ApplyPolicy(WebRequestMode mode)
        {
            // switch的回傳值 表達式
            WebRequestPolicyProvider.Policy = mode switch
            {
                WebRequestMode.Online   => OnlinePolicy,
                WebRequestMode.Offline  => OfflinePolicy,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}
