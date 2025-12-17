
using UnityEngine;

namespace XPlan.BuildTools.Runtime
{
    public interface IBuildConfigApplier
    {
        void Apply(BuildConfigSO config);
    }
}