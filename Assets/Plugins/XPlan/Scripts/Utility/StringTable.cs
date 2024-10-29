using TMPro;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace XPlan.Utility
{
    public class StringTable
	{
		private Dictionary<string, string> stringTable	= new Dictionary<string, string>();

		public StringTable()
		{

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
					string[] values = line.Split(',');

					if (values.Length != 2)
					{
						continue;
					}

					stringTable.Add(values[0], values[1]);
				}
			}
		}

		public void InitialUIText(GameObject uiGO)
		{
			Text[] textComponents = uiGO.GetComponentsInChildren<Text>(true);

			foreach (Text textComponent in textComponents)
			{				
				textComponent.text = GetStr(textComponent.text, false);
			}

			TextMeshProUGUI[] tmpTextComponents = uiGO.GetComponentsInChildren<TextMeshProUGUI>(true);
			foreach (TextMeshProUGUI tmpText in tmpTextComponents)
			{
				tmpText.text = GetStr(tmpText.text, false);
			}
		}

		public string GetStr(string keyStr, bool bShowWarning = false)
		{
			if (stringTable.ContainsKey(keyStr))
			{
				string originStr	= stringTable[keyStr];
				string processedStr = originStr.Replace("\\n", "\n");

				return processedStr;
			}

			if(bShowWarning)
			{
				Debug.LogWarning("字表中沒有此關鍵字 !!");
			}
			
			return keyStr;
		}
	}
}
