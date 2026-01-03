// ==============================================================================
// XPlan Framework
//
// Copyright (c) 2026 Asail
// All rights reserved.
//
// Author  : Asail0712
// Project : XPlan
// Description:
//     A modular framework for Unity projects, focusing on MVVM architecture,
//     runtime tooling, event-driven design, and extensibility.
//
// Contact : asail0712@gmail.com
// GitHub  : https://github.com/asail0712/XPlanDemo
//
// Unauthorized copying, modification, or distribution of this file,
// via any medium, is strictly prohibited without prior permission.
// ==============================================================================
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace XPlan.Editors
{
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Rendering;

    public class RemoveShadowsFromPrefab : MonoBehaviour
    {
        // 在 Unity 編輯器中創建一個菜單項
        [MenuItem("XPlanTools/Remove Shadows from Selected Prefab")]
        static void RemoveShadowsFromSelectedPrefab()
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
            Renderer[] renderers = prefabInstance.GetComponentsInChildren<Renderer>(true);

            // 記錄修改，允許撤銷操作
            Undo.RecordObject(prefabInstance, "Remove Shadows from Prefab");

            foreach (Renderer renderer in renderers)
            {
                // 禁用投影陰影
                renderer.shadowCastingMode = ShadowCastingMode.Off;

                // 禁用接收陰影
                renderer.receiveShadows = false;

                // 標記 Renderer 為已修改，準備保存
                EditorUtility.SetDirty(renderer);
            }

            // 保存 Prefab 修改
            PrefabUtility.SaveAsPrefabAsset(prefabInstance, prefabPath);
            PrefabUtility.UnloadPrefabContents(prefabInstance);

            Debug.Log("已成功移除 " + selectedObject.name + " 及其子物件的陰影，並保存到 Prefab。");
        }
    }

}