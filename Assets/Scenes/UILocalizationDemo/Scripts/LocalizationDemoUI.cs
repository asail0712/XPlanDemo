using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using XPlan.UI;
using XPlan.Utility;

namespace XPlan.Demo.Localization
{
	public class LocalizationDemoUI : UIBase
	{
		[Header("I18N處理")]
		[I18NView("DemoStr1")]
		[SerializeField] Text demoTxt1;
        [I18NView("DemoStr2")]
        [SerializeField] Text demoTxt2;
        [I18NView("DemoStr3")]
        [SerializeField] Text demoTxt3;


        protected override void OnRefreshLanguage(int currLang)
        {
            string currLangStr = string.Empty;

            switch (currLang)
            {
                case 0:
                    currLangStr = "繁體中文";
                    break;
                case 1:
                    currLangStr = "英文";
                    break;
                case 2:
                    currLangStr = "日文";
                    break;
            }

            LogSystem.Record($"Current Language is {currLangStr}");
        }


        [ContextMenu("換成中文")]
        private void ChangeChn()
        {
            StringTable.Instance.CurrLanguage = 0;
        }

        [ContextMenu("換成英文")]
        private void ChangeEng()
        {
            StringTable.Instance.CurrLanguage = 1;
        }

        [ContextMenu("換成日文")]
        private void ChangeJap()
        {
            StringTable.Instance.CurrLanguage = 2;
        }
    }
}