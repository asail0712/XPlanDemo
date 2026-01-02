using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PlayMeowDemo
{
    // 按鈕文字縮放與顏色變化效果
    // 用於按下與鬆開時提供視覺回饋
    public class ButtonStyle : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private Text _buttonTxt;       // 按鈕顯示文字

        [SerializeField] private float _originSize;     // 按鈕文字原始縮放大小
        [SerializeField] private float _triggerSize;    // 按下時文字縮放大小
        [SerializeField] private Color _originColor;    // 按鈕原始文字顏色
        [SerializeField] private Color _triggerColor;   // 按下時文字顏色

        // 按下時觸發，改變文字大小與顏色
        public void OnPointerDown(PointerEventData eventData)
        {
            _buttonTxt.transform.localScale  = new Vector3(_triggerSize, _triggerSize, _triggerSize);
            _buttonTxt.color                 = _triggerColor;
        }

        // 放開時還原文字大小與顏色
        public void OnPointerUp(PointerEventData eventData)
        {
            _buttonTxt.transform.localScale  = new Vector3(_originSize, _originSize, _originSize);
            _buttonTxt.color                 = _originColor;
        }
    }
}
