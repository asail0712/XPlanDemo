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
            Debug.Log($"[CecilWeaver] Enabled = {Enabled}");
        }

        // ★ 新增：手動觸發 Weave
        [MenuItem("XPlanTools/Weaver/Run Weaver Now", false, 0)]
        private static void RunWeaverNow()
        {
            // 直接呼叫公開 AP
            CecilWeaver.RunNow();

            Debug.Log("[CecilWeaver] 手動觸發 IL Weaving…");

            // 重新觸發編譯 → 重新 run weaving
            //AssetDatabase.SaveAssets();
            //UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
        }
    }
}
