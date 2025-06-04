using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace XPlan.Gesture
{
    public class DragToMove : MonoBehaviour
    {
        [Header("Drag Settings")]
        [SerializeField] public InputFingerMode fingerMode  = InputFingerMode.OneFinger;
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        [SerializeField] public MouseTrigger mouseTrigger   = MouseTrigger.LeftMouse;
#endif //UNITY_EDITOR
        [SerializeField] public bool bAllowPassThroughUI    = false;

        [Header("Clamp Settings")]
        [SerializeField] public bool bClampMove             = false;
        [SerializeField] public Vector3 minPosition         = new Vector3(-10, -10, -10);
        [SerializeField] public Vector3 maxPosition         = new Vector3(10, 10, 10);
        [SerializeField] public float screenToWorldRatio    = 0.01f;

        private float offsetZ               = -999f;
        private Vector3 defaultPos          = Vector3.zero;
        private Vector3 relativeDistance    = Vector3.zero;

        private Vector3 startWorldPos       = Vector3.zero;
        private Vector2 startScreenPos      = Vector2.zero;

        // 避免跟兩指縮放混淆
        private float lastTouchDistance     = 0;
        private bool bInputStart            = false;

        private void Awake()
		{
            if (Camera.main != null)
			{
                defaultPos      = transform.position;
                offsetZ         = Vector3.Distance(Camera.main.transform.position, transform.position);
                bInputStart     = false;
            }            
        }

		void Update()
        {
            // 检查是否有手指触摸屏幕
            if (!CheckInput() || !Camera.main)
            {
                return;
            }

            Vector3 touchPos = GetScreenPos();

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

            //LogSystem.Record($"World Pos {worldPosition}");

            // 在一次移動中 InputStart觸發一次 但是InputFinish要觸發很多次
            if (InputStart())
			{
                bInputStart         = true;

                // 計算點擊座標與物體的相對距離
                relativeDistance    = transform.position - worldPosition;
                startWorldPos       = transform.position;
                startScreenPos      = GetScreenPos();

                //LogSystem.Record($"World Pos {worldPosition}", LogType.Warning);
                //LogSystem.Record($"Relative Distance {relativeDistance}", LogType.Warning);
                //LogSystem.Record($"Input Start", LogType.Warning);

                if (Input.touchCount >= 2)
                {
                    lastTouchDistance   = Vector2.Distance(Input.GetTouch(0).position, Input.GetTouch(1).position);
                }
            }
            else if (InputFinish())
            {
                //LogSystem.Record($"Input Finish", LogType.Warning);

                Vector3 targetPos;

                if (bClampMove)
                {
                    Vector2 currentScreenPos    = GetScreenPos();
                    Vector2 dragDelta           = currentScreenPos - startScreenPos;

                    dragDelta.x                 = Mathf.Clamp(dragDelta.x, -1000f, 1000f);
                    dragDelta.y                 = Mathf.Clamp(dragDelta.y, -1000f, 1000f);

                    Vector3 offsetWorld         = new Vector3(-dragDelta.x * screenToWorldRatio, dragDelta.y * screenToWorldRatio, 0);

                    targetPos                   = startWorldPos + offsetWorld;

                    targetPos.x                 = Mathf.Clamp(targetPos.x, defaultPos.x + minPosition.x, defaultPos.x + maxPosition.x);
                    targetPos.y                 = Mathf.Clamp(targetPos.y, defaultPos.y + minPosition.y, defaultPos.y + maxPosition.y);
                    targetPos.z                 = Mathf.Clamp(targetPos.z, defaultPos.z + minPosition.z, defaultPos.z + maxPosition.z);
                }
                else
                {
                    // 不 Clamp 時完全跟隨手指
                    targetPos = worldPosition + relativeDistance;
                }

                transform.position  = targetPos;
            }
        }

        private Vector3 GetScreenPos()
		{
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
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
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            return Input.GetMouseButton(GestureTools.MouseKey(mouseTrigger));
#else
            return fingerMode == InputFingerMode.OneFinger ? Input.touchCount == 1 : Input.touchCount >= 2;
#endif
        }

        private bool InputStart()
		{
            if (bInputStart)
            {
                return false;
            }

            return CheckInput();
        }

        private bool InputFinish()
        {
            if(!bInputStart)
            {
                return false;
            }

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            return Input.GetMouseButton(GestureTools.MouseKey(mouseTrigger));
#else
            int fingerIndex = fingerMode == InputFingerMode.TwoFingers ? 1 : 0;
            Touch touch     = Input.GetTouch(fingerIndex);

            return touch.phase == TouchPhase.Moved;
#endif
        }

        private bool IsTwoFingerDrag()
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
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