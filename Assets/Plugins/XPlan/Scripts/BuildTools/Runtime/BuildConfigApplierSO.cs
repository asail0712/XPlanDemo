using System;
using UnityEngine;
using XPlan.BuildTools.Runtime;

namespace XPlan.BuildTools.Runtime
{
    public abstract  class BuildConfigApplierSO : ScriptableObject, IBuildConfigApplier
    {
        public abstract void Apply(BuildConfigSO config);
    }
}