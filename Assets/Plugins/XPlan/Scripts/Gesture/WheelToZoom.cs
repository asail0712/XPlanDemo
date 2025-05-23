using UnityEngine;

namespace XPlan.Gesture
{
    public class WheelToZoom : MonoBehaviour
    {
        [SerializeField] private float scaleSpeed   = 1.0f; // 控制縮放速度的變數
        [SerializeField] public float minScale      = 0.5f; // 最小縮放限制
        [SerializeField] public float maxScale      = 3.0f; // 最大縮放限制

        void Update()
        {
            // 取得滑鼠滾輪的輸入值
            float scroll            = Input.GetAxis("Mouse ScrollWheel");

            // 計算新的縮放值
            float newScale          = transform.localScale.x + scroll * scaleSpeed;

            // 限制縮放值在最小和最大範圍內
            newScale                = Mathf.Clamp(newScale, minScale, maxScale);

            // 設定物件的新縮放值
            transform.localScale    = new Vector3(newScale, newScale, newScale);
        }
    }
}
