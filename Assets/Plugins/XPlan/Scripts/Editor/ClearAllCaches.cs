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
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

#if ADDRESSABLES_EXISTS
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
#endif //ADDRESSABLES_EXISTS

namespace XPlan.Editors
{
    public class ClearAllCaches : MonoBehaviour
    {
        [MenuItem("XPlanTools/Clear All Cache")]
        public static void ClearAllCache()
        {
            bool cacheCleared = Caching.ClearCache(); // 清除全部本地快取（包括 Addressables）
            Debug.Log($"🧹 快取清除結果：{cacheCleared}");

#if ADDRESSABLES_EXISTS
            // 這行可清除 Addressables 的載入狀態
            var handle = Addressables.CleanBundleCache();

            handle.Completed += (AsyncOperationHandle<bool> op) =>
            {
                
            };

            Addressables.ClearResourceLocators();
            Debug.Log("Addressables 缓存已清除");

            string dir = Path.Combine(Application.persistentDataPath, "com.unity.addressables");
            if (Directory.Exists(dir))
            {
                Directory.Delete(dir, true);
                Debug.Log($"Deleted catalog folder: {dir}");
            }
            else
            {
                Debug.Log($"Catalog folder not found: {dir}");
            }

            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));

            // 1) ServerData：建議整個資料夾都清掉（避免不同平台殘留）
            string serverDataRoot = Path.Combine(projectRoot, "ServerData");
            if (Directory.Exists(serverDataRoot))
            {
                DeleteDirIfExists(serverDataRoot, "ServerData (ALL)");
            }
            else
            {
                Debug.Log("ℹ️ ServerData not found.");
            }


#endif //ADDRESSABLES_EXISTS
        }

        private static void DeleteDirIfExists(string path, string label)
        {
            try
            {
                if (Directory.Exists(path))
                {
                    File.SetAttributes(path, FileAttributes.Normal);
                    foreach (var file in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
                    {
                        File.SetAttributes(file, FileAttributes.Normal);
                    }
                    Directory.Delete(path, true);
                    Debug.Log($"🗑️ Deleted {label}: {path}");
                }
                else
                {
                    Debug.Log($"ℹ️ {label} not found: {path}");
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"⚠️ Failed to delete {label}: {path}\n{e}");
            }
        }
    }
}