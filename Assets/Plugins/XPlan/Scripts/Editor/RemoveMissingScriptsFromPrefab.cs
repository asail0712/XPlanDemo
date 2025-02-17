﻿using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

using XPlan.Utility;

namespace XPlan.Editors
{
    public class RemoveMissingScriptsFromPrefab : MonoBehaviour
    {
        [MenuItem("XPlanTools/Remove Missing Scripts From Prefab")]
        private static void RemoveMissingScriptsFromSelectedPrefab()
        {
            // 獲取當前選中的 GameObject
            GameObject selectedObject = Selection.activeGameObject;

            if (selectedObject == null)
            {
                Debug.LogWarning("請選擇一個 Prefab 或 GameObject");
                return;
            }

            // 確認選中的物件是 Prefab
            string prefabPath = AssetDatabase.GetAssetPath(selectedObject);
            if (string.IsNullOrEmpty(prefabPath) || !prefabPath.EndsWith(".prefab"))
            {
                Debug.LogWarning("請選擇一個有效的 Prefab");
                return;
            }

            // 加載 Prefab 並開始編輯
            GameObject prefabInstance = PrefabUtility.LoadPrefabContents(prefabPath);
            if (prefabInstance == null)
            {
                Debug.LogError("無法加載 Prefab: " + prefabPath);
                return;
            }

            // 遍歷 Prefab 中的所有子物件
            int totalMissCount = 0;

            List<GameObject> childGOList = prefabInstance.GetAllChildren();
            foreach (GameObject childGO in childGOList)
            {
                int missCount   = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(childGO);
                totalMissCount  += missCount;
            }

            if(totalMissCount > 0)
            {
                Debug.LogWarning($"{prefabInstance.name} 一共移除了 {totalMissCount} 個 miss script");
            }

            // 保存 Prefab 修改
            PrefabUtility.SaveAsPrefabAsset(prefabInstance, prefabPath);
            PrefabUtility.UnloadPrefabContents(prefabInstance);
        }
    }
}