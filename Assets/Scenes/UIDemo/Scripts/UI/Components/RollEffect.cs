using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PlayMeowDemo
{
    // 滑鼠/手指移入時顯示高亮效果，移出時隱藏。
    // 常用在輸入框或按鈕的「Roll Over」提示。
    public class RollEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Image _rollImg;    // 要顯示/隱藏的高亮 UI

        // 指標移入時觸發 → 顯示效果
        public void OnPointerEnter(PointerEventData eventData)
        {
            _rollImg.gameObject.SetActive(true);
        }

        // 指標移出時觸發 → 隱藏效果
        public void OnPointerExit(PointerEventData eventData)
        {
            _rollImg.gameObject.SetActive(false);
        }
    }
}
