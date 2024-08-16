using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XPlan.Gesture
{
    public class DragToMove : MonoBehaviour
    {
        private float zOffset               = 0f;
        private Vector3 relativeDistance    = Vector3.zero;

		private void Awake()
		{
            zOffset = Vector3.Distance(Camera.main.transform.position, transform.position);
        }

		void Update()
        {
            // 检查是否有一个手指触摸屏幕
            if (Input.touchCount == 1)
            {
                Touch touch             = Input.GetTouch(0);
                // 从屏幕坐标转换为世界坐标
                Vector3 screenPosition  = new Vector3(touch.position.x, touch.position.y, zOffset);
                Vector3 worldPosition   = Camera.main.ScreenToWorldPoint(screenPosition);

                Debug.DrawLine(worldPosition, transform.position, Color.red, Time.deltaTime);

                if (touch.phase == TouchPhase.Began)
				{
                    // 計算點擊座標與物體的相對距離
                    relativeDistance = transform.position - worldPosition;
                }
                else if (touch.phase == TouchPhase.Moved)
                {            
                    // 设置物体的位置为触摸位置
                    transform.position = worldPosition + relativeDistance;
                }
            }
        }
    }
}