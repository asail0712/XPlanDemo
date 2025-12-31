using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace XPlan
{
    public abstract class DDItemViewModelBase : ItemViewModelBase
    {
        public abstract IGhostPayload CreateGhostPayload();
    }

    public interface IGhostPayload { }
}
