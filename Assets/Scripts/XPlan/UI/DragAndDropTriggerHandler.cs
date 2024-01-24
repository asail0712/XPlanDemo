using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace XPlan.UI
{
    public class DragAndDropTriggerHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public Action<PointerEventData> beginDragDelegate, dragingDelgate, endDragDelegate;

        public void OnBeginDrag(PointerEventData eventData)
        {
            beginDragDelegate?.Invoke(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            dragingDelgate?.Invoke(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            endDragDelegate?.Invoke(eventData);
        }
    }
}