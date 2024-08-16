using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XPlan.Gesture
{
    public class DragToRotate : MonoBehaviour
    {
        [SerializeField] private float rotationSpeed = 0.2f; // 控制旋转速度

        private Vector2 previousTouchPosition;

        void Update()
        {
            if (Input.touchCount == 1)
            {
                Touch touch = Input.GetTouch(0);

                if (touch.phase == TouchPhase.Began)
                {
                    // 记录初始触控位置
                    previousTouchPosition = touch.position;
                }
                else if (touch.phase == TouchPhase.Moved)
                {
                    // 计算触控位置的变化
                    Vector2 touchDelta      = touch.position - previousTouchPosition;

                    // 根据触控移动量旋转对象
                    float rotationY         = -touchDelta.x * rotationSpeed;
                    float rotationX         = touchDelta.y * rotationSpeed;

                    transform.Rotate(Vector3.up, rotationY, Space.World);
                    transform.Rotate(Vector3.right, rotationX, Space.World);

                    // 更新之前的触控位置
                    previousTouchPosition   = touch.position;
                }
            }
        }
    }
}