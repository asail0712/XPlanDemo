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
            var current = Resources.Load<CurrentPlayConfig>(
                "XPlan/BuildTools/CurrentPlayConfig");

            if (!current || !current.so || !current.applierSO)
            {
                Debug.Log("[XPlan] No CurrentPlayConfig found.");
                return;
            }

            current.applierSO.Apply(current.so);
        }
    }
}