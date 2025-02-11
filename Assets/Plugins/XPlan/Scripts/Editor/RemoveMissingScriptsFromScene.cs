using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

namespace XPlan.Editors
{
    public class RemoveMissingScriptsFromScene : MonoBehaviour
    {
        [MenuItem("XPlanTools/Remove Missing Scripts From Scene")]
        private static void RemoveMissingScriptsFromAllSceneObjects()
        {
            // 取得當前活動場景的所有根物件
            GameObject[] rootObjects    = SceneManager.GetActiveScene().GetRootGameObjects();
            int totalRemoved            = 0;
            List<string> logList        = new List<string>();

            // 遍歷所有根物件並遞迴檢查子物件
            foreach (GameObject root in rootObjects)
            {
                RemoveMissingScriptsRecursive(root, ref totalRemoved, logList);
            }

            // 輸出結果到 Console
            Debug.Log($"總共移除了 {totalRemoved} 個缺失的腳本。");
            foreach (string log in logList)
            {
                Debug.Log(log);
            }
        }

        // 遞迴檢查物件及其子物件
        private static void RemoveMissingScriptsRecursive(GameObject go, ref int totalRemoved, List<string> logList)
        {            
            // 移除缺失的腳本
            int missingCount    = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
            totalRemoved        += missingCount;

            if(missingCount != 0)
            {
                logList.Add($"GameObject '{go.name}' 有 {missingCount} 個缺失的腳本，正在移除...");
            }
            
            // 遞迴檢查所有子物件
            foreach (Transform child in go.transform)
            {
                RemoveMissingScriptsRecursive(child.gameObject, ref totalRemoved, logList);
            }
        }
    }
}
