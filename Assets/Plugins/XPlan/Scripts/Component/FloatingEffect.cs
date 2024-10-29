using UnityEngine;

using XPlan.Utility;

namespace XPlan.Component
{ 
    public class FloatingEffect : MonoBehaviour
    {
        [SerializeField] public float mimfloatAmplitude    = 0.4f;  // 最小漂浮的振幅（上下漂浮的距離）
        [SerializeField] public float maxfloatAmplitude    = 0.6f;  // 最大漂浮的振幅（上下漂浮的距離）
        [SerializeField] public float minfloatSpeed        = 0.1f;  // 最小漂浮的速度
        [SerializeField] public float maxfloatSpeed        = 0.15f; // 最大漂浮的速度

        private Vector3 initialPosition;                            // 物件初始位置
        private float elapsedTime;                                  // 已經過去的時間
		private float realSpeed;
        private float realAmplitude;

        private void Start()
        {
            // 記錄物體的初始位置
            initialPosition = transform.position;

            Refresh();
        }

        public void Refresh()
		{
            realSpeed       = Random.Range(minfloatSpeed, maxfloatSpeed);
            realAmplitude   = Random.Range(mimfloatAmplitude, maxfloatAmplitude);
        }

        private void Update()
        {
            // 計算時間的進度，用於應用Easing效果
            elapsedTime         += Time.deltaTime;

            // 使用 Mathf.Sin 創建周期性的浮動效果
            float yOffset       = realAmplitude * Mathf.Sin(elapsedTime * realSpeed);
            
            // 更新物體位置
            transform.position  = new Vector3(
                initialPosition.x,
                initialPosition.y + yOffset,
                initialPosition.z
            );
        }
    }
}