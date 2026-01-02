using System.Collections;
using UnityEngine;

using XPlan;
using XPlan.UI;
using XPlan.Utility;

namespace Demo
{
    // 登入介面 UI：處理按鈕事件、輸入框、錯誤提示顯示等。
    // 透過 UIRequest 發送事件給 Presenter，並接收 UICommand 做顯示控制。
    public class LoginViewModel : ViewModelBase
    {
        private ObservableProperty<string> _account         = new ObservableProperty<string>();
        private ObservableProperty<string> _pw              = new ObservableProperty<string>();
        private ObservableProperty<bool> _loginViewVisible  = new(true);
        private ObservableProperty<string> _errorMsg        = new ObservableProperty<string>();

        private int _errorNotifyRoutine                     = -1; // 控制錯誤訊息顯示的 Coroutine

        [NotifyHandler]
        private void ShowLogin(ShowLoginMsg dummyMsg)
        {
            _loginViewVisible.Value = true;
            _account.Value          = "";
            _pw.Value               = "";
            _errorMsg.Value         = "";
        }

        [NotifyHandler]
        private void ShowError(LoginErrorMsg errorMsg)
        {
            ShowError(errorMsg.error);
        }

        private void ShowError(LoginError error)
        {
            if (_errorNotifyRoutine != -1)
            {
                StopCoroutine(_errorNotifyRoutine);
            }

            _errorNotifyRoutine = StartCoroutine(ShowError_Imp(error));
        }

        private IEnumerator ShowError_Imp(LoginError error)
        {
            _errorMsg.Value = GetErrorMsg(error);

            if (string.IsNullOrEmpty(_errorMsg.Value))
            {
                yield break;
            }

            LogSystem.Record($"登入錯誤: {_errorMsg.Value}", LogType.Warning);

            yield return new WaitForSeconds(CommonDefine.ErrorShowTime);

            _errorMsg.Value = "";
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

        /****************************************
         * 監控ui component 狀態改變觸發的函數
         * **************************************/
        [ButtonBinding]
        private void OnLoginClick()
        {
            string account  = _account.Value;
            string pw       = _pw.Value;

            // 檢查：帳號是否為空
            if (string.IsNullOrEmpty(account))
            {
                ShowError(LoginError.NoAccount);
                return;
            }

            // 檢查：密碼是否為空
            if (string.IsNullOrEmpty(pw))
            {
                ShowError(LoginError.NoPw);
                return;
            }

            // 提醒：
            // InputField 的 ContentType 在 iOS 設為 Email 時，可能影響手寫輸入功能；
            // 建議以 Standard 輸入並在程式端自行檢查格式。
            if (!account.IsValidEmail())
            {
                ShowError(LoginError.NotEmail);
                return;
            }

            // 檢查：密碼長度是否足夠
            if (pw.Length < CommonDefine.PwMinLen)
            {
                ShowError(LoginError.PwTooShort);
                return;
            }

            LogSystem.Record($"使用者要求登入, 帳號為 {account}, 密碼為 {pw}");
        }

        [ButtonBinding]
        private void OnGoogleLoginClick()
        {
            LogSystem.Record($"使用者要求使用Google登入");
        }

        [ButtonBinding]
        private void OnForgetPWClick()
        {
            LogSystem.Record($"使用者忘記自己的密碼");
        }

        [ButtonBinding]
        private void OnRegNewClick()
        {
            LogSystem.Record($"使用者要求註冊新帳號");
        }

        [ButtonBinding]
        private void OnPrivacyClick()
        {
            LogSystem.Record($"使用者要求查看隱私權");
        }

        [ButtonBinding]
        private void OnTcClick()
        {
            LogSystem.Record($"使用者查看服務條款");
        }

        [ButtonBinding]
        private void OnCloseClick()
        {
            LogSystem.Record($"使用者關掉Login UI");

            _loginViewVisible.Value = false;
        }

        [ButtonBinding]
        private void OnAccountChange(string account)
        {
            Debug.Log($"帳號輸入中 {account}");
        }

        [ButtonBinding]
        private void OnPwChange(string pw)
        {
            Debug.Log($"密碼輸入中 {pw}");
        }
    }
}