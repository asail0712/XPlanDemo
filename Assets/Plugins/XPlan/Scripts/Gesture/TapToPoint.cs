using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace XPlan.Gesture
{ 
    public class TapToPoint : MonoBehaviour
    {
        public Action<GameObject, Vector3> finishAction;

        void Update()
        {
            if (Input.touchCount == 1)
            {
                Touch touch = Input.GetTouch(0);

                if (touch.phase == TouchPhase.Began)
                {
                    // 从相机发出一条射线到触摸的位置
                    Ray ray = Camera.main.ScreenPointToRay(touch.position);
                    RaycastHit hitInfo;

                    // 检测射线是否碰到任何物体
                    if (Physics.Raycast(ray, out hitInfo))
                    {
						finishAction?.Invoke(hitInfo.collider.gameObject, hitInfo.point);
					}
                }
            }
        }
    }
}
