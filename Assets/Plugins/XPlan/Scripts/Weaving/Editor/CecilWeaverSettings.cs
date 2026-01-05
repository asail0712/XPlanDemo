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
using UnityEngine;
using UnityEditor;

namespace XPlan.Editors.Weaver
{
    public static class CecilWeaverSettings
    {
        private const string Key = "XPlan.CecilWeaver.Enabled";

        public static bool Enabled
        {
            get => EditorPrefs.GetBool(Key, true);
            set => EditorPrefs.SetBool(Key, value);
        }

        [MenuItem("XPlanTools/Weaver/Auto Weave Enabled", false, 9)]
        private static void Toggle()
        {
            Enabled = !Enabled;

            if (Enabled)
            {
                Debug.Log("🟢 Auto Cecil Weaver 已啟用");
            }
            else
            {
                Debug.Log("🔴 Auto Cecil Weaver 已停用");
            }
        }

        [MenuItem("XPlanTools/Weaver/Auto Weave Enabled", true)]
        private static bool ToggleValidate()
        {
            Menu.SetChecked("XPlanTools/Weaver/Auto Weave Enabled", Enabled);
            return true;
        }

        // ★ 新增：手動觸發 Weave
        [MenuItem("XPlanTools/Weaver/Run Weaver Now", false, 0)]
        private static void RunWeaverNow()
        {
#if WEAVING_ENABLE
            // 直接呼叫公開 AP
            CecilWeaver.RunNow();
            Debug.Log("[CecilWeaver] 手動觸發 IL Weaving…");
#else
            Debug.LogWarning("[CecilWeaver] 無法執行 Weaving，請先確認 WEAVING_ENABLE 符號已啟用！");
#endif // WEAVING_ENABLE

            // 重新觸發編譯 → 重新 run weaving
            //AssetDatabase.SaveAssets();
            //UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
        }
    }
}
