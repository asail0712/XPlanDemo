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
    public class TextToCSV : EditorWindow
    {
        [MenuItem("XPlanTools/Localization/Text To CSV")]
        static void Init()
        {
            TextToCSV window = (TextToCSV)EditorWindow.GetWindow(typeof(TextToCSV));
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("搜尋所有 Scene 的文字框:");

            if (GUILayout.Button("尋找"))
            {
                SearchTextObjects();
            }
        }

        private void SearchTextObjects()
        {
            string csvFilePath = "Assets/StringTable/TextData.csv"; // 指定 CSV 檔案的保存路徑
            StreamWriter sw = new StreamWriter(csvFilePath, false);

            // 獲取所有場景
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
                            Text[] textComponents = loadingInfo.uiPerfab.GetComponentsInChildren<Text>(true);

                            foreach (Text textComponent in textComponents)
                            {
                                // 這裡可以對找到的 Text 做相應的處理，例如列舉名稱、修改文字等
                                Debug.Log("在 Prefab " + loadingInfo.uiPerfab.name + " 中找到文字框：" + textComponent.gameObject.name + " 裡面寫著：" + textComponent.text);

                                string textContent      = textComponent.text;
                                string gameObjectName   = textComponent.gameObject.name;
                                string sceneName        = scene.name;

                                // 將找到的文字框內容輸出到 CSV 檔案中
                                sw.WriteLine(sceneName + "," + gameObjectName + "," + textContent);
                            }

                            TextMeshProUGUI[] TMPTextComponents = loadingInfo.uiPerfab.GetComponentsInChildren<TextMeshProUGUI>(true);
                            foreach (TextMeshProUGUI TMPText in TMPTextComponents)
                            {
                                // 這裡可以對找到的 Text 做相應的處理，例如列舉名稱、修改文字等
                                Debug.Log("在 Prefab " + loadingInfo.uiPerfab.name + " 中找到文字框：" + TMPText.gameObject.name + " 裡面寫著：" + TMPText.text);

                                string textContent      = TMPText.text;
                                string gameObjectName   = TMPText.gameObject.name;
                                string sceneName        = scene.name;

                                // 將找到的文字框內容輸出到 CSV 檔案中
                                sw.WriteLine(sceneName + "," + gameObjectName + "," + textContent);
                            }
                        }
                    }
                }
            }

            sw.Close();

            // 返回最初的場景
            EditorSceneManager.OpenScene(scenePaths[0], OpenSceneMode.Single);

            Debug.Log("文字框內容已輸出到 CSV 檔案: " + csvFilePath);
        }
    }
}