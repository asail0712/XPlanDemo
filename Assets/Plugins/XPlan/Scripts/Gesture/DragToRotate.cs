using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace XPlan.Gesture
{
    public class DragToRotate : MonoBehaviour
    {
#if UNITY_EDITOR
        [SerializeField] private MouseTrigger mouseTrigger  = MouseTrigger.LeftMouse;
#endif //UNITY_EDITOR
        [SerializeField] private bool bAllowPassThroughUI   = false;
        [SerializeField] private bool bOnlyRotateY          = true;
        [SerializeField] public bool bLocalRotate           = false;
        [SerializeField] public float rotationSpeed         = 0.05f; // 控制旋转速度

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
                    float rotationX         = touchDelta.y * rotationSpeed;
                    float rotationY         = -touchDelta.x * rotationSpeed;

                    if(!bOnlyRotateY)
                    {
                        if(bLocalRotate)
                        {
                            transform.Rotate(transform.right, rotationX, Space.World);
                        }
                        else
                        {
                            transform.Rotate(Vector3.right, rotationX, Space.World);
                        }                        
                    }

                    if (bLocalRotate)
                    {
                        transform.Rotate(transform.up, rotationY, Space.World);
                    }
                    else
                    { 
                        transform.Rotate(Vector3.up, rotationY, Space.World);
                    }

                    // 更新之前的触控位置
                    previousTouchPosition   = GetInputPos();
                }
            }
        }

#if UNITY_EDITOR
        private int MouseKey()
        {
            switch (mouseTrigger)
            {
                case MouseTrigger.LeftMouse:
                    return 0;
                case MouseTrigger.MiddleMouse:
                    return 2;
                case MouseTrigger.RightMouse:
                    return 1;
            }
            return 0;
        }
#endif //UNITY_EDITOR
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
            return Input.GetMouseButton(MouseKey());
#else
            return Input.touchCount == 1;
#endif
        }

        private bool InputStart()
        {
#if UNITY_EDITOR
            return Input.GetMouseButtonDown(MouseKey());
#else
            Touch touch = Input.GetTouch(0);
            return touch.phase == TouchPhase.Began;
#endif
        }

        private bool InputFinish()
        {
#if UNITY_EDITOR
            return Input.GetMouseButton(MouseKey());
#else
            Touch touch = Input.GetTouch(0);
            return touch.phase == TouchPhase.Moved;
#endif
        }
    }
}