using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace XPlan.Gesture
{
    public class DragToRotate : MonoBehaviour
    {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        [SerializeField] public MouseTrigger mouseTrigger   = MouseTrigger.LeftMouse;
#endif //UNITY_EDITOR
        [SerializeField] public bool bAllowPassThroughUI    = false;
        [SerializeField] public bool bOnlyRotateY           = true;
        [SerializeField] public bool bLocalRotate           = false;
        [SerializeField] public float rotationSpeed         = 0.05f; // 控制旋转速度
        [SerializeField] public bool bInverseX              = false;
        [SerializeField] public bool bInverseY              = false;

        [Header("Clamp Settings")]
        [SerializeField] public bool bClampRotation         = false;
        [SerializeField] public float minRotationX          = -90f;
        [SerializeField] public float maxRotationX          = 90f;
        [SerializeField] public float minRotationY          = -135f;
        [SerializeField] public float maxRotationY          = 135f;

        private Vector2 previousTouchPosition;

        void Update()
        {
            if (!CheckInput())
            {
                return;
            }

            Vector3 touchPos = GetInputPos();

            bool bIsOutOfScreen =
                touchPos.x < 0 ||
                touchPos.x > Screen.width ||
                touchPos.y < 0 ||
                touchPos.y > Screen.height;

            if (bIsOutOfScreen)
            {
                Debug.Log("Out Of Screen！");
                return;
            }

            if (!bAllowPassThroughUI && GestureTools.IsPointerOverUI())
            {
                Debug.Log("點擊到了 UI 元素");
                return;
            }

            if (InputStart())
            {
                // 记录初始触控位置
                previousTouchPosition = GetInputPos();
            }
            else if (InputFinish())
            {
                // 计算触控位置的变化
                Vector2 touchDelta      = GetInputPos() - previousTouchPosition;

                float deltaX            = bInverseX ? touchDelta.y : -touchDelta.y;
                float deltaY            = bInverseY ? touchDelta.x : -touchDelta.x;

                float rotationX         = deltaX * rotationSpeed;
                float rotationY         = deltaY * rotationSpeed;

                // 選擇用 local 或 world space 旋轉
                if(bLocalRotate)
                {
                    // Clamp 現有 rotation
                    Vector3 euler   = transform.localEulerAngles;

                    // 因為 euler 角度範圍是 0~360，要轉成 -180~180 處理 clamp 比較準
                    float currentX  = NormalizeAngle(euler.x);
                    float currentY  = NormalizeAngle(euler.y);

                    // Y軸旋轉
                    if (rotationY != 0)
                    {
                        if(bClampRotation)
                        {
                            currentY = Mathf.Clamp(currentY + rotationY, minRotationY, maxRotationY);
                        }
                        else
                        {
                            currentY = currentY + rotationY;
                        }                            
                    }

                    // X軸旋轉（如果允許）
                    if (!bOnlyRotateY && rotationX != 0)
                    {
                        if (bClampRotation)
                        {
                            currentX = Mathf.Clamp(currentX + rotationX, minRotationX, maxRotationX);
                        }
                        else
                        {
                            currentX = currentX + rotationX;
                        }   
                    }

                    transform.localEulerAngles = new Vector3(currentX, currentY, euler.z);
                }
                else
                {
                    // World space clamp
                    Vector3 worldEuler  = transform.rotation.eulerAngles;
                    float currentX      = NormalizeAngle(worldEuler.x);
                    float currentY      = NormalizeAngle(worldEuler.y);

                    if (rotationY != 0)
                    {
                        if (bClampRotation)
                        {
                            currentY = Mathf.Clamp(currentY + rotationY, minRotationY, maxRotationY);
                        }
                        else
                        {
                            currentY = currentY + rotationY;
                        }
                    }

                    if (!bOnlyRotateY && rotationX != 0)
                    {
                        if (bClampRotation)
                        {
                            currentX = Mathf.Clamp(currentX + rotationX, minRotationX, maxRotationX);
                        }
                        else
                        {
                            currentX = currentX + rotationX;
                        }
                    }

                    Quaternion clampedRotation  = Quaternion.Euler(currentX, currentY, worldEuler.z);
                    transform.rotation          = clampedRotation;
                }

                previousTouchPosition = GetInputPos();
            }
        }

        private float NormalizeAngle(float angle)
        {
            angle %= 360f;

            if (angle > 180f)
            {
                angle -= 360f;
            }

            return angle;
        }

        private Vector2 GetInputPos()
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            return Input.mousePosition;
#else
            Touch touch = Input.GetTouch(0);
            // 从屏幕坐标转换为世界坐标
            return touch.position;
#endif
        }


        private bool CheckInput()
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            return Input.GetMouseButton(GestureTools.MouseKey(mouseTrigger));
#else
            return Input.touchCount == 1;
#endif
        }

        private bool InputStart()
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            return Input.GetMouseButtonDown(GestureTools.MouseKey(mouseTrigger));
#else
            Touch touch = Input.GetTouch(0);
            return touch.phase == TouchPhase.Began;
#endif
        }

        private bool InputFinish()
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            return Input.GetMouseButton(GestureTools.MouseKey(mouseTrigger));
#else
            Touch touch = Input.GetTouch(0);
            return touch.phase == TouchPhase.Moved;
#endif
        }
    }
}