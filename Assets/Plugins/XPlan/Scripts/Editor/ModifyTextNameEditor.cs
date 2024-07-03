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
    public class ModifyTextNameEditor : EditorWindow
    {
        [MenuItem("Window/Modify Text Name")]
        static void Init()
        {
            ModifyTextNameEditor window = (ModifyTextNameEditor)EditorWindow.GetWindow(typeof(ModifyTextNameEditor));
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("搜尋所有 文字框:");

            if (GUILayout.Button("改名"))
            {
                CollectTextfieldObjects();
            }
        }

        private void CollectTextfieldObjects()
        {
            string csvFilePath      = "Assets/StringTable/TextData_Transtion.csv"; // 指定 CSV 檔案的保存路徑

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

            string[] scenePaths = EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes);

            foreach (string scenePath in scenePaths)
            {
                Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

                // 獲取場景中所有的 GameObject
                GameObject[] rootObjects = scene.GetRootGameObjects();

                foreach (GameObject rootObject in rootObjects)
                {
                    // 在每個 GameObject 中尋找 Text 組件
                    UILoader[] loaderComponents = rootObject.GetComponentsInChildren<UILoader>(true);

                    foreach (UILoader loader in loaderComponents)
                    {
                        List<UILoadingInfo> loadingList = loader.GetLoadingList();

                        foreach (UILoadingInfo loadingInfo in loadingList)
                        {
                            string assetPath    = AssetDatabase.GetAssetPath(loadingInfo.uiPerfab);
                            GameObject uiGO     = loadingInfo.uiPerfab;

                            Text[] textComponents = uiGO.GetComponentsInChildren<Text>(true);
                            foreach (Text textComponent in textComponents)
                            {
                                string textContent      = textComponent.text;
                                string gameObjectName   = textComponent.gameObject.name;
                                string sceneName        = scene.name;

                                foreach(List<string> rowData in data)
								{
                                    if (rowData[0] == sceneName && rowData[1] == gameObjectName && rowData[2] == textContent)
                                    {
                                        textComponent.gameObject.name = rowData[3];
                                    }
                                }                                
                            }

                            TextMeshProUGUI[] TMPTextComponents = uiGO.GetComponentsInChildren<TextMeshProUGUI>(true);
                            foreach (TextMeshProUGUI TMPText in TMPTextComponents)
                            {
                                string textContent      = TMPText.text;
                                string gameObjectName   = TMPText.gameObject.name;
                                string sceneName        = scene.name;

                                foreach (List<string> rowData in data)
                                {
                                    if (rowData[0] == sceneName && rowData[1] == gameObjectName && rowData[2] == textContent)
                                    {
                                        TMPText.gameObject.name = rowData[3];
                                    }
                                }
                            }

                            GameObject instanceRoot = (GameObject)PrefabUtility.InstantiatePrefab(uiGO);
                            PrefabUtility.SaveAsPrefabAsset(instanceRoot, assetPath, out bool success);
                        }
                    }
                }
            }
        }
    }
}