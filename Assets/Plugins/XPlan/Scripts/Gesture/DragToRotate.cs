using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace XPlan.Gesture
{
    public class DragToRotate : MonoBehaviour
    {
        [SerializeField] private bool bAllowPassThroughUI   = false;
        [SerializeField] private bool bOnlyRotateY          = true;
        [SerializeField] private float rotationSpeed        = 0.05f; // 控制旋转速度

        private Vector2 previousTouchPosition;

        void Update()
        {
            if (!bAllowPassThroughUI && EventSystem.current.IsPointerOverGameObject())
            {
                //Debug.Log("點擊到了 UI 元素");
                return;
            }

            if (CheckInput())
            {
                if (InputStart())
                {
                    // 记录初始触控位置
                    previousTouchPosition = GetInputPos();
                }
                else if (InputFinish())
                {
                    // 计算触控位置的变化
                    Vector2 touchDelta      = GetInputPos() - previousTouchPosition;

                    // 根据触控移动量旋转对象
                    float rotationX         = 0f;                    
                    float rotationY         = -touchDelta.x * rotationSpeed;
                    if(!bOnlyRotateY)
                    { 
                        rotationX           = touchDelta.y * rotationSpeed;
                    }

                    transform.Rotate(Vector3.up, rotationY, Space.World);
                    transform.Rotate(Vector3.right, rotationX, Space.World);

                    // 更新之前的触控位置
                    previousTouchPosition   = GetInputPos();
                }
            }
        }


        private Vector2 GetInputPos()
        {
#if UNITY_EDITOR
            return Input.mousePosition;
#else
            Touch touch = Input.GetTouch(0);
            // 从屏幕坐标转换为世界坐标
            return touch.position;
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