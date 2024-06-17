using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace XPlan.UI
{
    public class PointEventTriggerHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public Action<PointerEventData> OnPointDown, OnPointUp;

        public void OnPointerDown(PointerEventData eventData)
        {
            OnPointDown?.Invoke(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            OnPointUp?.Invoke(eventData);
        }
    }
}