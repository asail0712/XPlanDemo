using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace XPlan.Gesture
{
    public enum GesterType
    {
        None,
        Up,
        Down,
        Left,
        Right
    }

    public class TriggerGesture : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField]
        private float triggerDis = 200f;

        //private CanvasGroup canvasGroup;
        private Vector2 startDrapPos;
        private GesterType[] typeList;

        public Action<GesterType> onCallback;

        public void OnPointerDown(PointerEventData eventData)
        {
            // 取消 UI 的交互，避免拖曳時觸發其他事件
            //canvasGroup.blocksRaycasts  = false;
            startDrapPos                = eventData.pressPosition;

            Debug.Log($"Start Drap :{startDrapPos} ");
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            // 釋放時恢復 UI 的交互
            Vector2 stopDrapPos = eventData.position;

            Debug.Log($"Stop Drap :{stopDrapPos} ");

            foreach(GesterType type in typeList)
			{                
                Vector2 offset = stopDrapPos - startDrapPos;

                if (CanTrigger(type, offset))
				{
                    onCallback?.Invoke(type);
                }                
            }            
		}

        private bool CanTrigger(GesterType triggerType, Vector2 offset)
        {
            if (Vector2.Distance(offset, Vector2.zero) < triggerDis)
            {
                return false;
            }

            bool bIsTrigger = false;

            switch (triggerType)
            {
                case GesterType.Up:
                    if (offset.y > 0)
                    {
                        bIsTrigger = true;
                    }
                    break;
                case GesterType.Down:
                    if (offset.y < 0)
                    {
                        bIsTrigger = true;
                    }
                    break;
                case GesterType.Left:
                    if (offset.x > 0)
                    {
                        bIsTrigger = true;
                    }
                    break;
                case GesterType.Right:
                    if (offset.x < 0)
                    {
                        bIsTrigger = true;
                    }
                    break;
                case GesterType.None:
                    break;
            }

            return bIsTrigger;
        }
    }
}
