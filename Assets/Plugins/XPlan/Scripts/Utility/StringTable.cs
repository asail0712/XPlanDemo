using TMPro;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using XPlan.UI;

namespace XPlan.Utility
{
    public class StringTable
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
		private Dictionary<string, List<string>> stringTable	= new Dictionary<string, List<string>>();

		public StringTable()
		{
			currLang = 0;
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
			Text[] textComponents = uiGO.GetComponentsInChildren<Text>(true);

			foreach (Text textComponent in textComponents)
			{				
				textComponent.text = GetStr(textComponent.text);
			}

			TextMeshProUGUI[] tmpTextComponents = uiGO.GetComponentsInChildren<TextMeshProUGUI>(true);
			foreach (TextMeshProUGUI tmpText in tmpTextComponents)
			{
				tmpText.text = GetStr(tmpText.text);
			}
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

				UIBase[] uiList = uiIns.GetComponents<UIBase>();

				foreach (UIBase ui in uiList)
				{
					ui.RefreshText();
				}
			}
		}
	}
}
