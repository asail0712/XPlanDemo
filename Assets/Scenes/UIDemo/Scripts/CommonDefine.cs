namespace Demo
{
    // 共用常數定義（系統中可直接引用，不需實例化）
    public static class CommonDefine
    {
        public const int PwMinLen           = 6;        // 密碼最小長度
        public const float ErrorShowTime    = 3.5f;     // UI 顯示錯誤提示的時間（秒）
    }

    // UI 發出的「請求事件」名稱定義（由 View 觸發）
    public static class UIRequest
    {
        // login
        public static string Login              = "Login";
        public static string GoogleLogin        = "GoogleLogin";
        public static string RegisterNewAcc     = "RegisterNewAcc";
        public static string ForgetPassword     = "ForgetPassword";
        public static string Close              = "Close";

        // T&C
        public static string ShowPrivacy        = "ShowPrivacy";
        public static string ShowTC             = "ShowTC";
    }

    // 系統發出的「指令事件」名稱定義（由Presenter觸發）
    public static class UICommand
    {
        public static string OpenLogin          = "OpenLogin";
        public static string ShowLoginError     = "ShowLoginError";
    }
}
