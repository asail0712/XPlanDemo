using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace XPlan.Gesture
{ 
    public class PinchToZoom : MonoBehaviour
    {
        [SerializeField] public bool bAllowPassThroughUI    = false;
        [SerializeField] public float zoomInRatio           = 0.15f;
        [SerializeField] public float zoomOutRatio          = 0.25f;
        [SerializeField] public float editorZoomSpeed       = 0.05f; // 滾輪在Editor的縮放倍率

        [SerializeField] public float minScale              = 0.1f;
        [SerializeField] public float maxScale              = 5.0f;

        private float lastDist;
        private Vector3 lastScale;

        void Update()
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            HandleMouseZoom();
#else
            HandleTouchZoom();
#endif
        }

        private void HandleTouchZoom()
        {
            if (!bAllowPassThroughUI && GestureTools.IsPointerOverUI())
            {
                return;
            }

            if (Input.touchCount == 2)
            {
                Touch touch1 = Input.GetTouch(0);
                Touch touch2 = Input.GetTouch(1);

                if (touch1.phase == TouchPhase.Moved && touch2.phase == TouchPhase.Moved)
                {
                    if (lastDist == 0)
                    {
                        lastDist            = Vector2.Distance(touch1.position, touch2.position);
                        lastScale           = transform.localScale;
                    }

                    float newDist           = Vector2.Distance(touch1.position, touch2.position);
                    float zoomRatio         = newDist > lastDist ? zoomInRatio : zoomOutRatio;
                    float realZoomRatio     = 1 + (newDist - lastDist) / lastDist * zoomRatio;
                    Vector3 newScale        = lastScale * realZoomRatio;

                    // 加上上下限限制
                    float clampedX          = Mathf.Clamp(newScale.x, minScale, maxScale);
                    float clampedY          = Mathf.Clamp(newScale.y, minScale, maxScale);
                    float clampedZ          = Mathf.Clamp(newScale.z, minScale, maxScale);

                    transform.localScale    = new Vector3(clampedX, clampedY, clampedZ);
                }
                else
                {
                    lastDist = 0;
                }
            }
        }

        private void HandleMouseZoom()
        {
            if (!bAllowPassThroughUI && GestureTools.IsPointerOverUI())
            {
                return;
            }

            float scroll = Input.GetAxis("Mouse ScrollWheel");

            if (Mathf.Abs(scroll) > 0.01f)
            {
                float zoomFactor        = 1 + scroll * editorZoomSpeed;
                Vector3 newScale        = transform.localScale * zoomFactor;

                // 加上上下限限制
                float clampedX          = Mathf.Clamp(newScale.x, minScale, maxScale);
                float clampedY          = Mathf.Clamp(newScale.y, minScale, maxScale);
                float clampedZ          = Mathf.Clamp(newScale.z, minScale, maxScale);

                transform.localScale    = new Vector3(clampedX, clampedY, clampedZ);
            }
        }
    }
}
