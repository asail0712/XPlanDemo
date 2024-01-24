using UnityEngine;

namespace XPlan.Interface
{
    public interface IScrollItem
    {
        void SetContentPos(Vector2 pos);

        int CenterPos { get; set; }
    }
}