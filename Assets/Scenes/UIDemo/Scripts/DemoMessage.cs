using UnityEngine;
using XPlan.Observe;

namespace Demo
{
    // 登入錯誤類型列舉：用於 Presenter 與 UI 之間傳遞錯誤狀態
    public enum LoginError
    {
        None = 0,
        NoAccount,
        NoPw,
        NotEmail,
        PwTooShort,
        AccountOrPWDeny,
    }

    // 要求顯示登入 UI 的訊息（Model/系統 → Presenter → UI）
    public class ShowLoginMsg : MessageBase
    {
        public ShowLoginMsg()
        {
            // 無需攜帶資料，單純通知顯示登入 UI
        }
    }

    // 登入錯誤訊息（Model/系統 → Presenter → UI）
    public class LoginErrorMsg : MessageBase
    {
        public LoginError error;

        public LoginErrorMsg(LoginError error)
        {
            this.error = error;
        }
    }

    public class AddItemDescMsg : MessageBase
    {
        private static int i = 1;

        public string desc;
        public AddItemDescMsg() 
        {
            this.desc = $"這是第 {i++} 個Item";
        }
    }
}