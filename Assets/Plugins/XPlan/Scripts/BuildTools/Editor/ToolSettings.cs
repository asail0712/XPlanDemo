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
