using UnityEngine;

namespace XPlan.UI.Component
{

    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaComponent : MonoBehaviour
    {
#if UNITY_EDITOR
        [SerializeField] private float simulatedSafeAreaTop = 102f;
#endif //UNITY_EDITOR

        private RectTransform panel;
        private Rect lastSafeArea                   = new Rect(0, 0, 0, 0);
        private ScreenOrientation lastOrientation   = ScreenOrientation.AutoRotation;
        private Vector2Int lastScreenSize           = new Vector2Int(0, 0);

        void Awake()
        {
            panel = GetComponent<RectTransform>();
            ApplySafeArea();
        }

        void Update()
        {
            if (HasScreenChanged())
            { 
                ApplySafeArea();
            }
        }

#if UNITY_EDITOR
        // iPhone 14 Pro 模擬用的 Safe Area 資料（動態島）
        private Rect SimulatedSafeArea()
        {
            // 設定解析度對應的 Safe Area (以 1179x2556 螢幕為例)
            float screenWidth   = Screen.width;
            float screenHeight  = Screen.height;

            float safeX = 0f;
            float safeY = 0f; // 類似動態島的高度
            float safeW = screenWidth - safeX;
            float safeH = screenHeight - simulatedSafeAreaTop;

            return new Rect(safeX, safeY, safeW, safeH);
        }
#endif

        private void ApplySafeArea()
        {
#if UNITY_EDITOR
            Rect safeArea       = SimulatedSafeArea();
#else
            Rect safeArea       = Screen.safeArea;
#endif

            Vector2 anchorMin   = safeArea.position;
            Vector2 anchorMax   = safeArea.position + safeArea.size;

            anchorMin.x         /= Screen.width;
            anchorMin.y         /= Screen.height;
            anchorMax.x         /= Screen.width;
            anchorMax.y         /= Screen.height;

            panel.anchorMin     = anchorMin;
            panel.anchorMax     = anchorMax;

            lastSafeArea        = Screen.safeArea;
            lastOrientation     = Screen.orientation;
            lastScreenSize.x    = Screen.width;
            lastScreenSize.y    = Screen.height;
        }

        private bool HasScreenChanged()
        {
#if UNITY_EDITOR
            Rect checkArea  = SimulatedSafeArea();
#else
            Rect checkArea  = Screen.safeArea;
#endif

            return lastSafeArea     != checkArea ||
                   lastOrientation  != Screen.orientation ||
                   lastScreenSize.x != Screen.width ||
                   lastScreenSize.y != Screen.height;
        }
    }
}