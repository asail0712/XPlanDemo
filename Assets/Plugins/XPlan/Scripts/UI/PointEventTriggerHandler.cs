using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace XPlan.UI
{
    public class PointEventTriggerHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public Action<PointerEventData> OnPointDown, OnPointUp, OnPointEnter, OnPointExit;

        public void OnPointerDown(PointerEventData eventData)
        {
            OnPointDown?.Invoke(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            OnPointUp?.Invoke(eventData);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            OnPointEnter?.Invoke(eventData);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            OnPointExit?.Invoke(eventData);
        }
    }
}