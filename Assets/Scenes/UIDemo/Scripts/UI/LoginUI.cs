using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using XPlan;
using XPlan.UI;

namespace Demo.MVP
{
    // 登入介面 UI：處理按鈕事件、輸入框、錯誤提示顯示等。
    // 透過 UIRequest 發送事件給 Presenter，並接收 UICommand 做顯示控制。
    public class LoginUI : UIBase
    {
        [Header("I18N處理")]
        [I18NView("TitleLogo")]
        [SerializeField] private Image _titleLogo;
        [I18NView("PlzInputEmail")]
        [SerializeField] private Text _plzInputEmail;
        [I18NView("PlzInputPW")]
        [SerializeField] private Text _plzInputPW;
        [I18NView("Login")]
        [SerializeField] private Text _login;
        [I18NView("ForgetPW")]
        [SerializeField] private Text _forgetPW;
        [I18NView("RegNew")]
        [SerializeField] private Text _regNew;
        [I18NView("PlzInputIdentity")]
        [SerializeField] private Text _plzInputIdentity;
        [I18NView("LoginWithOther")]
        [SerializeField] private Text _loginWithOther;
        [I18NView("Privacy")]
        [SerializeField] private Text _privacy;
        [I18NView("T&C")]
        [SerializeField] private Text _tc;

        [Header("登入按鈕組")]
        [SerializeField] private Button _loginBtn;
        [SerializeField] private Button _googleBtn;     
        [SerializeField] private Button _regNewBtn;
        [SerializeField] private Button _forgetPWBtn;

        [Header("輸入欄位")]
        [SerializeField] private InputField _accountTxt;
        [SerializeField] private InputField _pwTxt;

        [Header("錯誤訊息處理")]
        [SerializeField] private Text _errorTxt;

        [Header("條款相關")]
        [SerializeField] private Button _privacyBtn;
        [SerializeField] private Button _tcBtn;

        [Header("其他")]
        [SerializeField] private Button _closeBtn;

        private Coroutine _errorNotifyRoutine;              // 控制錯誤訊息顯示的 Coroutine

        // Start is called before the first frame update
        private void Awake()
        {
            /******************************
             * 使用者與 UI 互動 → 發送 UIRequest
             * ***************************/
            RegisterButton("", _loginBtn, Logining);
            RegisterButton(UIRequest.GoogleLogin, _googleBtn);
            RegisterButton(UIRequest.RegisterNewAcc, _regNewBtn);
            RegisterButton(UIRequest.ForgetPassword, _forgetPWBtn);
            RegisterButton(UIRequest.ShowPrivacy, _privacyBtn);
            RegisterButton(UIRequest.ShowTC, _tcBtn);
            RegisterButton(UIRequest.Close, _closeBtn, () => 
            {
                ToggleUI(gameObject, false);
            });

            /******************************
             * Presenter → 發送 UICommand → UI 執行
             * ***************************/
            ListenCall(UICommand.OpenLogin, () =>
            {
                // 顯示 Login UI
                ToggleUI(gameObject, true);
            });

            ListenCall<string>(UICommand.ShowLoginError, (errorStr) => 
            {
                // 顯示登入錯誤訊息
                NotifyError(errorStr);
            });
        }

        // UI 每次啟用時的初始狀態
        private void OnEnable()
        {
            Initialized();
        }

        private void OnDisable()
        {
            if (_errorNotifyRoutine != null)
            {
                StopCoroutine(_errorNotifyRoutine);

                _errorTxt.text      = "";
                _errorNotifyRoutine = null;
            }
        }

        // 初始化輸入框與錯誤訊息
        private void Initialized()
        {
            NotifyError("");
            _accountTxt.text    = "";
            _pwTxt.text         = "";
        }

        // 按下登入按鈕 → 收集帳密 → 丟給 Presenter 判斷
        private void Logining()
        {
            NotifyError("");

            string account  = _accountTxt.text;
            string pw       = _pwTxt.text;

            // 傳遞帳密給邏輯層（Presenter）
            DirectTrigger<(string, string)>(UIRequest.Login, (account, pw));
        }

        // 顯示錯誤訊息（會自動計時並清除）
        private void NotifyError(string errorStr)
        {
            if(_errorNotifyRoutine != null)
            {
                StopCoroutine(_errorNotifyRoutine);
                
                _errorTxt.text      = "";
                _errorNotifyRoutine = null;
            }

            if(!gameObject.activeSelf)
            {
                return;
            }

            _errorNotifyRoutine = StartCoroutine(ChangeErrorMsg(errorStr));
        }

        // 錯誤訊息顯示 + 倒數清除
        private IEnumerator ChangeErrorMsg(string errorStr)
        {
            _errorTxt.text = errorStr;

            if(string.IsNullOrEmpty(errorStr))
            {
                yield break;
            }

            LogSystem.Record($"登入錯誤: {errorStr}", LogType.Warning);

            yield return new WaitForSeconds(CommonDefine.ErrorShowTime);

            _errorTxt.text = "";
        }

        /********************************
         * I18N相關處理
         * *****************************/
        protected override void OnRefreshLanguage(int currLang)
        {
            // 更換語系時同時觸發初始化
            Initialized();
        }
    }
}