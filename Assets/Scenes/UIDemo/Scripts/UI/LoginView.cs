using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using XPlan;
using XPlan.UI;

namespace Demo.MVVM 
{
    // 登入介面 UI：處理按鈕事件、輸入框、錯誤提示顯示等。
    [ViewBinding]
    public class LoginView : ViewBase<LoginViewModel>
    {        
        [Header("I18N處理")]
        [I18NView("TitleLogo")]
        [SerializeField] private Image _titleLogo;
        [I18NView("PlzInputEmail")]
        [SerializeField] private Text _plzInputEmailTxt;
        [I18NView("PlzInputPW")]
        [SerializeField] private Text _plzInputPWTxt;
        [I18NView("Login")]
        [SerializeField] private Text _loginTxt;
        [I18NView("ForgetPW")]
        [SerializeField] private Text _forgetPWTxt;
        [I18NView("RegNew")]
        [SerializeField] private Text _regNewTxt;
        [I18NView("PlzInputIdentity")]
        [SerializeField] private Text _plzInputIdentityTxt;
        [I18NView("LoginWithOther")]
        [SerializeField] private Text _loginWithOtherTxt;
        [I18NView("Privacy")]
        [SerializeField] private Text _privacyTxt;
        [I18NView("T&C")]
        [SerializeField] private Text _tcTxt;

        [Header("登入按鈕組")]
        [SerializeField] private Button _loginBtn;
        [SerializeField] private Button _googleLoginBtn;
        [SerializeField] private Button _regNewBtn;
        [SerializeField] private Button _forgetPWBtn;

        [Header("輸入欄位")]
        [SerializeField] private InputField _accountTxt;
        [SerializeField] private InputField _pwTxt;

        [Header("錯誤訊息處理")]
        [SerializeField] private Text _errorMsgTxt;

        [Header("條款相關")]
        [SerializeField] private Button _privacyBtn;
        [SerializeField] private Button _tcBtn;

        [Header("其他")]
        [SerializeField] private Button _closeBtn;
    }
}