using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace XPlan.Gesture
{
    public class DragToMove : MonoBehaviour
    {
#if !UNITY_IOS && !UNITY_ANDROID
        [SerializeField] private MouseTrigger mouseTrigger  = MouseTrigger.LeftMouseKey;
#endif
        [SerializeField] private bool bAllowPassThroughUI   = false;

        private float zOffset               = -999f;
        private Vector3 relativeDistance    = Vector3.zero;

		private void Awake()
		{
            if (Camera.main != null)
			{
                zOffset = Vector3.Distance(Camera.main.transform.position, transform.position);
            }            
        }

		void Update()
        {
            if (!bAllowPassThroughUI && EventSystem.current.IsPointerOverGameObject())
            {
                //Debug.Log("點擊到了 UI 元素");
                return;
            }

            // 检查是否有一个手指触摸屏幕
            if (CheckInput() && Camera.main)
            {
                if(zOffset == -999f)
				{
                    zOffset = Vector3.Distance(Camera.main.transform.position, transform.position);
                }

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

        private int MouseKey()
        {
            switch (mouseTrigger)
            {
                case MouseTrigger.LeftMouseKey:
                    return 0;
                case MouseTrigger.MiddleMouseKey:
                    return 2;
                case MouseTrigger.RightMouseKey:
                    return 1;
            }
            return 0;
        }

        private Vector3 GetScreenPos()
		{
#if !UNITY_IOS && !UNITY_ANDROID
            return new Vector3(Input.mousePosition.x, Input.mousePosition.y, zOffset);
#else
            Touch touch = Input.GetTouch(0);
            // 从屏幕坐标转换为世界坐标
            return new Vector3(touch.position.x, touch.position.y, zOffset);
#endif
        }

        private bool CheckInput()
		{
#if !UNITY_IOS && !UNITY_ANDROID
            return Input.GetMouseButton(MouseKey());
#else
            return Input.touchCount == 1;
#endif
        }

        private bool InputStart()
		{
#if !UNITY_IOS && !UNITY_ANDROID
            return Input.GetMouseButtonDown(MouseKey());
#else
            Touch touch = Input.GetTouch(0);
            return touch.phase == TouchPhase.Began;
#endif
        }

        private bool InputFinish()
        {
#if !UNITY_IOS && !UNITY_ANDROID
            return Input.GetMouseButton(MouseKey());
#else
            Touch touch = Input.GetTouch(0);
            return touch.phase == TouchPhase.Moved;
#endif
        }
    }
}