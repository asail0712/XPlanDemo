using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XPlan.Component
{

	public class DestroyWhenNoVision : MonoBehaviour
	{
		[SerializeField]
		private float noVisionDuration	    = 2.0f;

        [SerializeField]
        public bool bTriggerCallback        = false;

        private float elapsedTime		    = 0f;
		private bool bIsObjectInViewport    = false;
        private Coroutine destroyCoroutine  = null;
        public Action<GameObject> onDestroy;

        void Update()
        {
            // 每幀更新計時器
            elapsedTime += Time.deltaTime;

            // 檢查物體是否在畫面上
            bIsObjectInViewport = IsObjectInViewport();

            // 如果在畫面上，重置計時器
            if (bIsObjectInViewport)
            {
                elapsedTime = 0f;
            }

            // 如果物體在畫面上並且計時器超過兩秒，啟動協程進行延遲刪除
            if (!bIsObjectInViewport && elapsedTime > noVisionDuration)
            {
                destroyCoroutine = StartCoroutine(DelayedDestroyCoroutine());
            }
        }

        private void OnDestroy()
        {
            if (destroyCoroutine != null)
            {
                StopCoroutine(destroyCoroutine);
            }
        }

        IEnumerator DelayedDestroyCoroutine()
        {
            // 等待兩秒
            yield return new WaitForSeconds(1f);

            if(bTriggerCallback)
			{
                onDestroy?.Invoke(gameObject);
            }
            else
			{
                // 刪除物體
                Destroy(gameObject);
            }
        }

        bool IsObjectInViewport()
        {
            // 將物體的位置轉換為螢幕座標
            Vector3 screenPoint = Camera.main.WorldToViewportPoint(transform.position);

            // 檢查物體是否在視圖內
            return (screenPoint.x >= 0 && screenPoint.x <= 1 && screenPoint.y >= 0 && screenPoint.y <= 1);
        }
    }
}