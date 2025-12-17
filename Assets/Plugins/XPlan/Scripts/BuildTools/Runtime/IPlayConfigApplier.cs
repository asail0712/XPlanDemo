
using UnityEngine;

namespace XPlan.BuildTools.Runtime
{
    public interface IPlayConfigApplier
    {
        void Apply(PlayConfigSO config);
    }
}