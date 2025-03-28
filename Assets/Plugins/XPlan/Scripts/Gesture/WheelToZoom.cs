using UnityEngine;

namespace XPlan.Gesture
{
    public class WheelToZoom : MonoBehaviour
    {
        [SerializeField] private float scaleSpeed   = 1.0f; // 北罽硉跑计
        [SerializeField] public float minScale      = 0.5f; // 程罽
        [SerializeField] public float maxScale      = 3.0f; // 程罽

        void Update()
        {
            // 眔菲公簎近块
            float scroll            = Input.GetAxis("Mouse ScrollWheel");

            // 璸衡穝罽
            float newScale          = transform.localScale.x + scroll * scaleSpeed;

            // 罽程㎝程絛瞅ず
            newScale                = Mathf.Clamp(newScale, minScale, maxScale);

            // 砞﹚ン穝罽
            transform.localScale    = new Vector3(newScale, newScale, newScale);
        }
    }
}
