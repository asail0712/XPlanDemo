using TMPro;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using XPlan.UI;

namespace XPlan.Editors
{
    public class CollectTextEditor : EditorWindow
    {
        [MenuItem("XPlanTools/Localization/Generate CHT CSV")]
        static void Init()
        {
            CollectTextEditor window = (CollectTextEditor)EditorWindow.GetWindow(typeof(CollectTextEditor));
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("搜尋所有 文字翻譯:");

            if (GUILayout.Button("生成"))
            {
                CollectTextObjects();
            }
        }

        private void CollectTextObjects()
        {
            string csvFilePath      = "Assets/StringTable/TextData_Transtion.csv"; // 指定 CSV 檔案的保存路徑
            string outCsvFilePath   = "Assets/StringTable/StringTable_CHT.csv"; // 指定 CSV 檔案的保存路徑

            List<List<string>> data = new List<List<string>>();

            using (StreamReader reader = new StreamReader(csvFilePath))
            {
                while (!reader.EndOfStream)
                {
                    string line     = reader.ReadLine();
                    string[] values = line.Split(',');
                    data.Add(new List<string>(values));
                }
            }

            Dictionary<string, string> stringTable = new Dictionary<string, string>();

            foreach(List<string> csvRow in data)
			{
                if(csvRow.Count == 4)
				{
                    if(stringTable.ContainsKey(csvRow[3]))
					{
                        if(stringTable[csvRow[3]] != csvRow[2])
                        {
                            Debug.Log("Something wrong !!");
                        }
					}
					else
					{
                        stringTable.Add(csvRow[3], csvRow[2]);
                    }
                }
            }

            using (StreamWriter writer = new StreamWriter(outCsvFilePath))
            {
                foreach (var pair in stringTable)
                {
                    string key      = pair.Key;
                    string value    = pair.Value;

                    // Replace special characters to avoid CSV parsing issues
                    key     = key.Replace(",", "\\,");
                    value   = value.Replace(",", "\\,");

                    // Write the key and value to the CSV file
                    writer.WriteLine($"{key},{value}");
                }
            }


        }
    }
}