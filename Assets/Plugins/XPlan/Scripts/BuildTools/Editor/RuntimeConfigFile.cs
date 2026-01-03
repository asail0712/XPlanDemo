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
#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace XPlan.BuildTools.Editors
{
    /// <summary>
    /// Editor 專用：把 runtime 設定內容寫成檔案
    /// 不解析、不理解內容，只負責 IO。
    /// </summary>
    public static class RuntimeConfigFile
    {
        public const string DefaultFileName = "xplan_runtime_config.json";

        public static void WriteJson(string jsonText, string fileName = DefaultFileName)
        {
            if (string.IsNullOrWhiteSpace(jsonText))
            {
                Debug.LogWarning("[XPlan] RuntimeConfigFile.WriteJson called with empty content.");
                return;
            }

            var folder = Path.Combine(Application.dataPath, "StreamingAssets");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            var path = Path.Combine(folder, fileName);
            File.WriteAllText(path, jsonText);

            AssetDatabase.Refresh();
            Debug.Log($"[XPlan] Runtime config written: {path}");
        }
    }
}
#endif
