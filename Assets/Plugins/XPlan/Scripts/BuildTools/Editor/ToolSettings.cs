#if UNITY_EDITOR
using UnityEditor;
using XPlan.BuildTools.Runtime;

namespace XPlan.BuildTools.Editors
{
    [FilePath("ProjectSettings/XPlanBuildToolsSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    public class ToolSettings : ScriptableSingleton<ToolSettings>
    {
        public BuildConfig buildConfig;
        public PlayConfig playConfig;

        public void Save() => Save(true);
    }
}
#endif
