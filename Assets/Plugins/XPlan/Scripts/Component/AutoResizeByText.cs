using UnityEngine;
using UnityEngine.UI;

namespace XPlan.Components
{
    [RequireComponent(typeof(Text))]
    [ExecuteAlways]
    public class AutoResizeByText : MonoBehaviour
    {
        private Text text;
        private RectTransform rectTransform;

        void Awake()
        {
            text            = GetComponent<Text>();
            rectTransform   = GetComponent<RectTransform>();
        }

        void Update()
        {
            if (text == null || rectTransform == null)
            {
                return;
            }

            float fixedWidth                = rectTransform.rect.width;
            TextGenerationSettings settings = text.GetGenerationSettings(new Vector2(fixedWidth, float.PositiveInfinity));
            float height                    = text.cachedTextGeneratorForLayout.GetPreferredHeight(text.text, settings) / text.pixelsPerUnit;

            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }
    }
}