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

        [Header("Product Settings")]
        public string productName;

        // ===============================
        // Build Options（開關）
        // ===============================
        [Header("Build Options")]
        [Tooltip("等同於 Unity Build Settings 的 Development Build")]
        public bool developmentBuild = false;

        [Tooltip("允許 Script Debugging（通常搭配 Development Build）")]
        public bool scriptDebugging = false;

        [Tooltip("允許 Profiler 連線（通常搭配 Development Build）")]
        public bool connectProfiler = false;

        [Tooltip("Deep Profiling（非常慢，除非必要不建議開）")]
        public bool deepProfiling = false;

        private string _prevProductName;

        // ===============================
        // BuildPlayerOptions 入口
        // ===============================
        public BuildPlayerOptions CreateBuildPlayerOptions()
        {
            var options = new BuildPlayerOptions
            {
                scenes              = GetScenes(),
                locationPathName    = GetBuildPath(),
                target              = GetBuildTarget(),
                targetGroup         = GetBuildTargetGroup(),
                options             = GetBuildOptions()
            };

            _prevProductName = PlayerSettings.productName;

            if (!string.IsNullOrEmpty(productName))
                PlayerSettings.productName = productName;

            OnBeforeBuild(options);
            return options;
        }

        public void RestoreAfterBuild()
        {
            if (!string.IsNullOrEmpty(_prevProductName))
                PlayerSettings.productName = _prevProductName;
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
        {
            var opts = BuildOptions.None;

            if (developmentBuild)
            {
                opts |= BuildOptions.Development;

                if (scriptDebugging)
                    opts |= BuildOptions.AllowDebugging;

                if (connectProfiler)
                    opts |= BuildOptions.ConnectWithProfiler;

                if (deepProfiling)
                    opts |= BuildOptions.EnableDeepProfilingSupport;
            }

            return opts;
        }

        /// <summary>
        /// 給子類最後調整用（例如切換 scripting backend、defines）
        /// </summary>
        protected virtual void OnBeforeBuild(BuildPlayerOptions options)
        {
        }
    }
}
