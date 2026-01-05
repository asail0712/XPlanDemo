using UnityEngine;

using XPlan;
using XPlan.UI;
using XPlan.Utility;

namespace Demo
{
    // 系統層的進入點，負責初始注入邏輯與提供測試按鍵（ContextMenu）操作
    public class PlayMeowSystem : SystemBase
    {
        // 系統初始化時會自動呼叫，用來註冊需要的 Presenter（邏輯層）
        protected override void OnPreInitial()
        {
            GameViewSizeForce.EnsureAndUseFixed("XPlan.Demo", 1440, 2960);
        }

        protected override void OnInitialLogic()
        {
            RegisterLogic(new LoginPresenter());
            RegisterLogic(new LoginViewModel());
        }

        /*****************************************
         * 在 Inspector 右鍵選單中 建立以下功能
         * **************************************/
        [ContextMenu("要求顯示Login UI")]
        private void ShowLoginUI()
        {
            LogSystem.Record("使用者要求開啟Login UI");

            new ShowLoginMsg().Send();
        }

        [ContextMenu("登入失敗,帳號或是密碼有誤")]
        private void SendLoginDeny()
        {
            LogSystem.Record("使用者登入失敗,帳號或是密碼有誤");

            new LoginErrorMsg(LoginError.AccountOrPWDeny).Send();
        }

        [ContextMenu("更換語系為中文")]
        private void SendLanguageChangeCHT()
        {
            LogSystem.Record("使用者更換語系為中文");

            UIController.Instance.CurrLanguage = 0;
        }

        [ContextMenu("更換語系為英文")]
        private void SendLanguageChangeENG()
        {
            LogSystem.Record("使用者更換語系為英文");

            UIController.Instance.CurrLanguage = 1;
        }

        [ContextMenu("更換Quality為Low")]
        private void SendQualityChangeLow()
        {
            LogSystem.Record("使用者更換Quality為Low");

            UIController.Instance.CurrQuality = 0;
        }

        [ContextMenu("更換Quality為Medium")]
        private void SendQualityChangeMedium()
        {
            LogSystem.Record("使用者更換Quality為Medium");

            UIController.Instance.CurrQuality = 1;
        }

        [ContextMenu("更換Quality為High")]
        private void SendQualityChangeHigh()
        {
            LogSystem.Record("使用者更換Quality為High");

            UIController.Instance.CurrQuality = 2;
        }
    }
}