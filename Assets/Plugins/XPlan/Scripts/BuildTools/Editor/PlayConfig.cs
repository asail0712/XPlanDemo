using UnityEngine;

namespace XPlan.BuildTools.Editors
{
    public abstract class PlayConfig : ScriptableObject
    {
        public string configId;
        public virtual string DisplayName => string.IsNullOrEmpty(configId) ? name : $"{configId} ({name})";
        public abstract string ExportRuntimeJson(); // 專案端自行決定內容
    }
}
