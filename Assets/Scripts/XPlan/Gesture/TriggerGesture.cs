using UnityEngine;
using UnityEngine.EventSystems;

namespace XPlan.Gesture
{
	public class TriggerGesture : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        //private CanvasGroup canvasGroup;
        private Vector2 startDrapPos;
        private GestureAction[] gestureActions;

        private void Start()
        {
            //canvasGroup     = GetComponent<CanvasGroup>();
            gestureActions  = GetComponents<GestureAction>();
        }

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
            //canvasGroup.blocksRaycasts  = true;
            Vector2 stopDrapPos         = eventData.position;

            Debug.Log($"Stop Drap :{stopDrapPos} ");

            foreach(GestureAction gestureAction in gestureActions)
			{
                if (gestureAction == null)
                {
                    continue;
                }

                Vector2 offset = stopDrapPos - startDrapPos;

                if (gestureAction.CanTrigger(offset))
				{
                    gestureAction.TriggerAction();
                }                
            }            
		}
	}
}
