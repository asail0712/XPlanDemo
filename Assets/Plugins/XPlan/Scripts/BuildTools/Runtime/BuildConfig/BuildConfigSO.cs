using UnityEngine;

namespace XPlan.BuildTools.Runtime
{
    public abstract class BuildConfigSO : ScriptableObject
    {
        public string configId;
        public virtual string DisplayName => string.IsNullOrEmpty(configId) ? name : $"{configId} ({name})";
        public BuildConfigApplierSO applier;
    }
}
