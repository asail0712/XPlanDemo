using UnityEngine;

namespace XPlan.BuildTools.Runtime
{
    public class CurrentBuildConfig : ScriptableObject
    {
        public BuildConfigSO so;
        public BuildConfigApplierSO applierSO;
    }

    public class CurrentPlayConfig : ScriptableObject
    {
        public PlayConfigSO so;
        public PlayConfigApplierSO applierSO;
    }
}
