using System;
using UnityEngine;

namespace XPlan.BuildTools.Runtime
{
    public abstract  class PlayConfigApplierSO : ScriptableObject, IPlayConfigApplier
    {
        public abstract void Apply(PlayConfigSO config);
    }
}