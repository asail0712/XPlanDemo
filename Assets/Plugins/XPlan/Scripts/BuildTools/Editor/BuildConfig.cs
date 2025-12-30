using System.Linq;
using UnityEditor;
using UnityEngine;

namespace XPlan.BuildTools.Editors
{
    public abstract class BuildConfig : ScriptableObject
    {
        [Header("基本資訊")]
        public string displayName;

        public virtual string DisplayName
            => string.IsNullOrEmpty(displayName) ? name : displayName;

        // ===============================
        // BuildPlayerOptions 入口
        // ===============================
        public BuildPlayerOptions CreateBuildPlayerOptions()
        {
            var options = new BuildPlayerOptions
            {
                scenes = GetScenes(),
                locationPathName = GetBuildPath(),
                target = GetBuildTarget(),
                targetGroup = GetBuildTargetGroup(),
                options = GetBuildOptions()
            };

            OnBeforeBuild(options);
            return options;
        }

        // ===============================
        // 可覆寫區（模板方法）
        // ===============================
        protected virtual string[] GetScenes()
        {
            return EditorBuildSettings.scenes
                .Where(s => s.enabled)
                .Select(s => s.path)
                .ToArray();
        }

        protected abstract string GetBuildPath();

        protected abstract BuildTarget GetBuildTarget();

        protected virtual BuildTargetGroup GetBuildTargetGroup()
            => BuildPipeline.GetBuildTargetGroup(GetBuildTarget());

        protected virtual BuildOptions GetBuildOptions()
            => BuildOptions.None;

        /// <summary>
        /// 給子類最後調整用（例如切換 scripting backend、defines）
        /// </summary>
        protected virtual void OnBeforeBuild(BuildPlayerOptions options)
        {
        }
    }
}
