using UnityEditor;
using UnityEngine;

namespace XPlan.BuildTools.Runtime
{
    public static class BuildPlayRuntimeBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void OnRuntimeStart()
        {
            ApplyCurrentBuildConfig();
            ApplyCurrentPlayConfig();
        }

        private static void ApplyCurrentBuildConfig()
        {
            var current = Resources.Load<CurrentBuildConfig>(
                "XPlan/BuildTools/CurrentBuildConfig");

            if (!current || !current.so || !current.applierSO)
            {
                Debug.Log("[XPlan] No CurrentBuildConfig found.");
                return;
            }

            current.applierSO.Apply(current.so);
        }

        private static void ApplyCurrentPlayConfig()
        { 
            const string CurrentPlayConfigAssetPath = "Assets/BuildTools/CurrentPlayConfig.asset";
            CurrentPlayConfig current               = AssetDatabase.LoadAssetAtPath<CurrentPlayConfig>(CurrentPlayConfigAssetPath);

            if (!current || !current.so || !current.applierSO)
            {
                Debug.Log("[XPlan] No CurrentPlayConfig found.");
                return;
            }

            current.applierSO.Apply(current.so);
        }
    }
}