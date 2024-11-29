using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace XPlan.Gesture
{ 
    public class PinchToZoom : MonoBehaviour
    {
        [SerializeField] private bool bAllowPassThroughUI   = false;
        [SerializeField] public float zoomInRatio           = 0.15f;
        [SerializeField] public float zoomOutRatio          = 0.25f;

        private float lastDist;
        private Vector3 lastScale;

        void Update()
        {
            if (!bAllowPassThroughUI && EventSystem.current.IsPointerOverGameObject())
            {
                //Debug.Log("點擊到了 UI 元素");
                return;
            }

            if (Input.touchCount == 2)
            {
                Touch touch1 = Input.GetTouch(0);
                Touch touch2 = Input.GetTouch(1);

                // 不使用TouchPhase.Began，因為不一定兩個touch會同時觸發began
                if (touch1.phase == TouchPhase.Moved && touch2.phase == TouchPhase.Moved)
                {
                    if (lastDist == 0)
                    {
                        lastDist    = Vector2.Distance(touch1.position, touch2.position);
                        lastScale   = gameObject.transform.localScale;
                    }

                    float newDist       = Vector2.Distance(touch1.position, touch2.position);
                    float zoomRatio     = newDist > lastDist ? zoomInRatio : zoomOutRatio;
                    float realZoomRatio = 1 + (newDist - lastDist) / lastDist * zoomRatio;
                    Vector3 newScale    = lastScale * realZoomRatio;

                    gameObject.transform.localScale = newScale;
                }
                else
				{
                    lastDist = 0;
                }
            }
        }
    }
}
