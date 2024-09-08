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
            if (CheckInput() && Camera.main)
            {
                // 从屏幕坐标转换为世界坐标
                Vector3 worldPosition = Camera.main.ScreenToWorldPoint(GetScreenPos());

                //Debug.DrawLine(worldPosition, transform.position, Color.red, Time.deltaTime);

                if (InputStart())
				{
                    // 計算點擊座標與物體的相對距離
                    relativeDistance = transform.position - worldPosition;
                }
                else if (InputFinish())
                {            
                    // 设置物体的位置为触摸位置
                    transform.position = worldPosition + relativeDistance;
                }
            }
        }

        private Vector3 GetScreenPos()
		{
#if UNITY_EDITOR
            return new Vector3(Input.mousePosition.x, Input.mousePosition.y, zOffset);
#else
            Touch touch = Input.GetTouch(0);
            // 从屏幕坐标转换为世界坐标
            return new Vector3(touch.position.x, touch.position.y, zOffset);
#endif
        }

        private bool CheckInput()
		{
#if UNITY_EDITOR
            return Input.GetMouseButton(0);
#else
            return Input.touchCount == 1;
#endif
        }

        private bool InputStart()
		{
#if UNITY_EDITOR
            return Input.GetMouseButtonDown(0);
#else
            Touch touch = Input.GetTouch(0);
            return touch.phase == TouchPhase.Began;
#endif
        }

        private bool InputFinish()
        {
#if UNITY_EDITOR
            return Input.GetMouseButton(0);
#else
            Touch touch = Input.GetTouch(0);
            return touch.phase == TouchPhase.Moved;
#endif
        }
    }
}