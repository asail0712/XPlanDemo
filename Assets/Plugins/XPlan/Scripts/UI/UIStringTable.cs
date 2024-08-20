using TMPro;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using XPlan.Utility;

namespace XPlan.UI
{
    public class UIStringTable : CreateSingleton<UIStringTable>
	{
		[SerializeField]
		public TextAsset csvAsset;

		//private const string csvFilePath				= "Assets/StringTable/StringTable.csv"; // 指定 CSV 檔案的保存路徑
		private Dictionary<string, string> stringTable	= new Dictionary<string, string>();

		protected override void InitSingleton()
		{
			if (csvAsset == null)
			{
				return;
			}

			string fileContent = csvAsset.text;

			string[] lines = fileContent.Split('\n'); // 將文件內容分成行

			foreach(string line in lines)
			{ 
				string[] values = line.Split(',');

				if (values.Length != 2)
				{
					continue;
				}

				stringTable.Add(values[0], values[1]);
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

		public string GetStr(string keyStr, bool bShowWarning = true)
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
