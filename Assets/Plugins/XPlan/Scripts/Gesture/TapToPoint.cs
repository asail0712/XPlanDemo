﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using XPlan.Observe;

namespace XPlan.Gesture
{ 
    public class TapToPointMsg : MessageBase
	{
        public GameObject hitGO;
        public Vector3 hitPos;
        public Vector3 hitNormal;

        public TapToPointMsg(GameObject hitGO, Vector3 hitPos, Vector3 hitNormal)
		{
            this.hitGO      = hitGO;
            this.hitPos     = hitPos;
            this.hitNormal  = hitNormal;
        }
    }

    public class TapToPoint : MonoBehaviour
    {
        [SerializeField] private GameObject specifyHitGO;

        public Action<GameObject, Vector3, Vector3> finishAction;

        void Update()
        {
            if (CheckInput())
            {
                if (InputStart())
                {
                    // 从相机发出一条射线到触摸的位置
                    Ray ray = Camera.main.ScreenPointToRay(GetInputPos());
                    RaycastHit hitInfo;

                    // 检测射线是否碰到任何物体
                    if (Physics.Raycast(ray, out hitInfo))
                    {
                        if (specifyHitGO == null)
						{
                            specifyHitGO = gameObject;
                        }

                        // hit到指定物體上才要處理後續
                        if (specifyHitGO != hitInfo.collider.gameObject)
                        {
                            return;
                        }

                        TapToPointMsg msg = new TapToPointMsg(hitInfo.collider.gameObject, hitInfo.point, hitInfo.normal);
                        msg.Send();

                        finishAction?.Invoke(hitInfo.collider.gameObject, hitInfo.point, hitInfo.normal);
					}
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
    }
}
