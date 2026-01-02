using XPlan;
using XPlan.Observe;
using XPlan.Utility;

namespace Demo
{
    // 登入流程的 Presenter：
    // 1) 接收 View 的使用者操作（UIRequest）並驗證基本輸入。
    // 2) 依據狀態對 UI 發送指令（UICommand）或記錄 Log。
    // 3) 接收 Model/系統的通知（RegisterNotify）並轉發給 UI。
    public class LoginPresenter : LogicComponent
    {        
        // Start is called before the first frame update
        public LoginPresenter()
        {
            /**********************************************
             * 接收 View 的回應（UI → Presenter）
             * 使用 AddUIListener 監聽 UIRequest
             **********************************************/
            AddUIListener<(string, string)>(UIRequest.Login, (pair) => 
            {
                string account  = pair.Item1;   // 使用者輸入帳號
                string pw       = pair.Item2;   // 使用者輸入密碼

                // 檢查：帳號是否為空
                if (string.IsNullOrEmpty(account))
                {
                    DirectCallUI<string>(UICommand.ShowLoginError, GetErrorMsg(LoginError.NoAccount));
                    return;
                }

                // 檢查：密碼是否為空
                if (string.IsNullOrEmpty(pw))
                {
                    DirectCallUI<string>(UICommand.ShowLoginError, GetErrorMsg(LoginError.NoPw));
                    return;
                }

                // 提醒：
                // InputField 的 ContentType 在 iOS 設為 Email 時，可能影響手寫輸入功能；
                // 建議以 Standard 輸入並在程式端自行檢查格式。
                if (!account.IsValidEmail())
                {
                    DirectCallUI<string>(UICommand.ShowLoginError, GetErrorMsg(LoginError.NotEmail));
                    return;
                }

                // 檢查：密碼長度是否足夠
                if (pw.Length < CommonDefine.PwMinLen)
                {
                    DirectCallUI<string>(UICommand.ShowLoginError, GetErrorMsg(LoginError.PwTooShort));
                    return;
                }

                LogSystem.Record($"使用者要求登入, 帳號為 {account}, 密碼為 {pw}");
            });

            /*******************************************************************
             * 其餘 UI 操作事件（Google 登入 / 註冊 / 忘記密碼 / 關閉 / 查看條款）
             * *****************************************************************/

            AddUIListener(UIRequest.GoogleLogin, () =>
            {
                LogSystem.Record($"使用者要求使用Google登入");
            });

            AddUIListener(UIRequest.RegisterNewAcc, () =>
            {
                LogSystem.Record($"使用者要求註冊新帳號");
            });

            AddUIListener(UIRequest.ForgetPassword, () =>
            {
                LogSystem.Record($"使用者忘記自己的密碼");
            });

            AddUIListener(UIRequest.Close, () =>
            {
                LogSystem.Record($"使用者關掉Login UI");
            });

            AddUIListener(UIRequest.ShowPrivacy, () =>
            {
                LogSystem.Record($"使用者要求查看隱私權");
            });

            AddUIListener(UIRequest.ShowTC, () =>
            {
                LogSystem.Record($"使用者查看服務條款");
            });

            /***************************************************
             * 接收 Model/系統 通知（Model/System → Presenter）
             * 使用 RegisterNotify 監聽 MessageBase
             ***************************************************/
            RegisterNotify<LoginErrorMsg>((msg) => 
            {
                DirectCallUI<string>(UICommand.ShowLoginError, GetErrorMsg(msg.error));
            });

            RegisterNotify<ShowLoginMsg>((dummy) =>
            {
                DirectCallUI(UICommand.OpenLogin);
            });
        }

        // 依照錯誤列舉取得多語系字串（Key → 字串）
        // 實際顯示的內容由 GetStr(key) 取得
        private string GetErrorMsg(LoginError error)
        {
            string msg = string.Empty;

            switch (error)
            {
                case LoginError.None:
                    msg = string.Empty;
                    break;
                case LoginError.NoAccount:
                    msg = GetStr("KEY_NoAccount");
                    break;
                case LoginError.NoPw:
                    msg = GetStr("KEY_NoPW");
                    break;
                case LoginError.NotEmail:
                    msg = GetStr("KEY_NotEmail");
                    break;
                case LoginError.PwTooShort:
                    msg = GetStr("KEY_PwTooShort");
                    break;
                case LoginError.AccountOrPWDeny:
                    msg = GetStr("KEY_AccountOrPWDeny");
                    break;
                default:
                    msg = GetStr("KEY_OtherError");
                    break;
            }

            return msg;
        }
    }
}
