using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace XPlan.Gesture
{
    public class DragToMove : MonoBehaviour
    {
        [Header("Drag Settings")]
        [SerializeField] private InputFingerMode fingerMode = InputFingerMode.OneFinger;
#if UNITY_EDITOR
        [SerializeField] private MouseTrigger mouseTrigger  = MouseTrigger.LeftMouse;
#endif //UNITY_EDITOR
        [SerializeField] private bool bAllowPassThroughUI   = false;

        [Header("Clamp Settings")]
        [SerializeField] private bool bClampMove            = false;
        [SerializeField] public Vector3 minPosition         = new Vector3(-10, -10, -10);
        [SerializeField] public Vector3 maxPosition         = new Vector3(10, 10, 10);
        [SerializeField] private Vector2 screenDragRange    = Vector2.zero;

        private float offsetZ               = -999f;
        private Vector3 defaultPos          = Vector3.zero;
        private Vector3 relativeDistance    = Vector3.zero;

        // 避免跟兩指縮放混淆
        private float lastTouchDistance     = 0;

        private void Awake()
		{
            if (Camera.main != null)
			{
                defaultPos          = transform.position;
                offsetZ             = Vector3.Distance(Camera.main.transform.position, transform.position);
                screenDragRange.x   = Mathf.Clamp(screenDragRange.x, 100f, Screen.width);
                screenDragRange.y   = Mathf.Clamp(screenDragRange.y, 100f, Screen.height);
            }            
        }

		void Update()
        {
            if (!bAllowPassThroughUI && EventSystem.current.IsPointerOverGameObject())
            {
                //Debug.Log("點擊到了 UI 元素");
                return;
            }

            // 检查是否有手指触摸屏幕
            if (!CheckInput() || !Camera.main)
            {
                return;
            }

            if (fingerMode == InputFingerMode.TwoFingers && !IsTwoFingerDrag())
            { 
                return;
            }

            if (offsetZ == -999f)
            {
                offsetZ = Vector3.Distance(Camera.main.transform.position, transform.position);
            }

            // 从屏幕坐标转换为世界坐标
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(GetScreenPos());

            Debug.DrawLine(worldPosition, transform.position, Color.red, Time.deltaTime);

            if (InputStart())
			{
                // 計算點擊座標與物體的相對距離
                relativeDistance = transform.position - worldPosition;

                if (Input.touchCount >= 2 && fingerMode == InputFingerMode.TwoFingers)
                {
                    lastTouchDistance = Vector2.Distance(Input.GetTouch(0).position, Input.GetTouch(1).position);
                }
            }
            else if (InputFinish())
            {
                Vector3 targetPos;

                if (bClampMove)
                {
                    // 使用螢幕範圍做比例計算
                    float normalizedX   = GetScreenPos().x / Screen.width;  // 0 左 → 1 右
                    float normalizedY   = GetScreenPos().y / Screen.height; // 0 下 → 1 上

                    float mappedX       = Mathf.Lerp(defaultPos.x + maxPosition.x, defaultPos.x + minPosition.x, normalizedX);
                    float mappedY       = Mathf.Lerp(defaultPos.y + minPosition.y, defaultPos.y + maxPosition.y, normalizedY);
                    float mappedZ       = Mathf.Clamp(transform.position.z, defaultPos.z + minPosition.z, defaultPos.z + maxPosition.z); // z 不處理比例（一般不會用來滑動）

                    targetPos = new Vector3(mappedX, mappedY, mappedZ);
                }
                else
                {
                    // 不 Clamp 時完全跟隨手指
                    targetPos = worldPosition + relativeDistance;
                }

                transform.position  = targetPos;
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

        private Vector3 GetScreenPos()
		{
#if UNITY_EDITOR
            return new Vector3(Input.mousePosition.x, Input.mousePosition.y, offsetZ);
#else
        
            float x = 0f;
            float y = 0f;

            for(int i = 0; i < Input.touchCount; ++i)
            {
                Touch touch = Input.GetTouch(i);

                x += touch.position.x;
                y += touch.position.y;
            }

            return new Vector3(x / Input.touchCount, y / Input.touchCount, offsetZ);
#endif
        }

        private bool CheckInput()
		{
#if UNITY_EDITOR
            return Input.GetMouseButton(MouseKey());
#else
            return fingerMode == InputFingerMode.OneFinger ? Input.touchCount == 1 : Input.touchCount >= 2;
#endif
        }

        private bool InputStart()
		{
#if UNITY_EDITOR
            return Input.GetMouseButtonDown(MouseKey());
#else
            int fingerIndex = fingerMode == InputFingerMode.TwoFingers ? 1 : 0;
            Touch touch     = Input.GetTouch(fingerIndex);

            return touch.phase == TouchPhase.Began;
#endif
        }

        private bool InputFinish()
        {
#if UNITY_EDITOR
            return Input.GetMouseButton(MouseKey());
#else
            int fingerIndex = fingerMode == InputFingerMode.TwoFingers ? 1 : 0;
            Touch touch     = Input.GetTouch(fingerIndex);

            return touch.phase == TouchPhase.Moved;
#endif
        }

        private bool IsTwoFingerDrag()
        {
#if UNITY_EDITOR
            return true;
#else
            if (Input.touchCount < 2) 
            {
                return false;
            }

            Touch touch0 = Input.GetTouch(0);
            Touch touch1 = Input.GetTouch(1);

            Vector2 move0 = touch0.deltaPosition;
            Vector2 move1 = touch1.deltaPosition;

            float similarity        = Vector2.Dot(move0.normalized, move1.normalized);
            float currentDistance   = Vector2.Distance(touch0.position, touch1.position);
            float distanceDelta     = Mathf.Abs(currentDistance - lastTouchDistance);

            // 更新 last distance 為下一幀做比較
            lastTouchDistance       = currentDistance;

            return similarity > 0.95f && distanceDelta < 10f; // 同方向且距離變化小 = 非縮放
#endif
        }
    }
}