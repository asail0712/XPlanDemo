using UnityEngine;

namespace XPlan.Component
{
    // 將Content居中
    public class ContentRealignment : MonoBehaviour
    {
        private void Awake()
        {
            RectTransform parentRect    = transform.parent.GetComponent<RectTransform>();
            RectTransform currentRect   = GetComponent<RectTransform>();

            if (parentRect == null || currentRect == null)
            {
                return;
            }

			float widthOffset               = currentRect.rect.width - parentRect.rect.width;
            Vector2 anchorPos               = currentRect.anchoredPosition;
            anchorPos.x                     = (widthOffset > 0) ? widthOffset / 2 : 0;
            currentRect.anchoredPosition    = anchorPos;
        }
    }
}