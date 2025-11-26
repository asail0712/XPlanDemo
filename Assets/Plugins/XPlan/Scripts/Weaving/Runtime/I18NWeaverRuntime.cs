using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XPlan.UI;
using XPlan.UI.Components;
using XPlan.Utility; // 如果有 StringTable.Instance 等

namespace XPlan.Weaver.Runtime
{
    public static class I18NWeaverRuntime
    {
        public static void RegisterText(MonoBehaviour view, object fieldValue, string key)
        {
            if (view == null || fieldValue == null || string.IsNullOrEmpty(key))
                return;

            var go              = view.gameObject;
            var textProvider    = go.AddOrFindComponent<I18NTextProvider>();

            switch (fieldValue)
            {
                case Text uiText:
                    textProvider.Register(uiText, key);
                    break;
                case TextMeshProUGUI tmp:
                    textProvider.Register(tmp, key);
                    break;
                default:
                    Debug.LogError($"[I18NWeaverRuntime] I18NTextAttribute 用在不支援的型別 {fieldValue.GetType()}");
                    break;
            }
        }

        public static void RegisterImage(MonoBehaviour view, Image img, string key)
        {
            if (view == null || img == null || string.IsNullOrEmpty(key))
                return;

            var go              = view.gameObject;
            var spriteProvider  = go.AddOrFindComponent<I18NSpriteProvider>();

            spriteProvider.Register(img, key);
        }

        public static void RefreshAll(MonoBehaviour view)
        {
            var go = view.gameObject;

            var textProvider    = go.GetComponent<I18NTextProvider>();
            var spriteProvider  = go.GetComponent<I18NSpriteProvider>();

            textProvider?.RefreshText();
            spriteProvider?.RefreshImage();
        }
    }
}
