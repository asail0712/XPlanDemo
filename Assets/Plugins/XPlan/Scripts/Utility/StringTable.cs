using TMPro;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using XPlan.UI;
using XPlan.UI.Components;

namespace XPlan.Utility
{
    public class StringTable : CreateSingleton<StringTable>
    {
		public int CurrLanguage
		{
			get
			{
				return currLang;
			}
			set
			{
				currLang = value;
				RefreshUILang();
			}
		}

		private int currLang									= -1;
		private Dictionary<string, List<string>> stringTable	= null;

        protected override void InitSingleton()
        {
            currLang	= 0;
            stringTable = new Dictionary<string, List<string>>(System.StringComparer.OrdinalIgnoreCase);
        }

        public void InitialStringTable(TextAsset[] csvAssetList)
		{
			if (csvAssetList == null)
			{
				return;
			}

			foreach (TextAsset csvAsset in csvAssetList)
			{
				string fileContent	= csvAsset.text;
				string[] lines		= fileContent.Split('\n'); // 將文件內容分成行

				foreach (string line in lines)
				{
					int index = line.IndexOf(',');

					if (index == -1)
					{
						continue;
					}

					string key		= line.Substring(0, index);
					string content	= line.Substring(index + 1);

					if (stringTable.ContainsKey(key))
					{
						List<string> strList = stringTable[key];

						strList.Add(content);
						stringTable[key] = strList;
					}
					else
					{
						List<string> strList = new List<string>();

						strList.Add(content);
						stringTable.Add(key, strList);
					}
				}
			}
		}

		public void InitialUIText(GameObject uiGO)
		{
            I18NTextProvider textKeycomp	= uiGO.AddOrFindComponent<I18NTextProvider>();
            I18NSpriteProvider i18Ncomp		= uiGO.AddOrFindComponent<I18NSpriteProvider>();
            
			textKeycomp.RefreshText();
            i18Ncomp.RefreshImage();
        }

		public string GetStr(string keyStr)
		{
			if (!stringTable.ContainsKey(keyStr))
			{
				//Debug.LogWarning("字表中沒有此關鍵字 !!");
			
				// 使用原本的字串
				return keyStr;
			}

			List<string> strList = stringTable[keyStr];

			if(strList.Count == 0)
            {
				// 使用原本的字串
				return keyStr;
			}

			if (currLang >= 0 && strList.Count > currLang)
			{
				string originStr	= strList[currLang];
				string processedStr = originStr.Replace("\\n", "\n");

				return processedStr;
			}

			// 使用第一個語系的字
			return strList[0];
		}

		public string ReplaceStr(string keyStr, params string[] paramList)
		{
			if (!stringTable.ContainsKey(keyStr))
			{
				// 使用原本的字串
				return keyStr;
			}

			List<string> strList = stringTable[keyStr];

			if (strList.Count == 0)
			{
				// 使用原本的字串
				return keyStr;
			}

			if (currLang < 0 && strList.Count <= currLang)
			{
				// 使用原本的字串
				return keyStr;
			}

			string originStr	= strList[currLang];
			string processedStr = originStr.Replace("\\n", "\n");

			for (int i = 0; i < paramList.Length; ++i)
			{
				string replaceStr	= $"[Param{i}]";
				processedStr		= processedStr.Replace(replaceStr, paramList[i]);
			}

			return processedStr;
		}

		private void RefreshUILang()
		{
			List<GameObject> allVisibleUIs = UIController.Instance.GetAllVisibleUI();

			foreach (GameObject uiIns in allVisibleUIs)
			{
				InitialUIText(uiIns);

                List<IUIView> uiList = uiIns.GetInterfaces<IUIView>();

				foreach (IUIView ui in uiList)
				{
					ui.RefreshLanguage(currLang);
				}
			}
		}
	}
}
