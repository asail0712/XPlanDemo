using System;
using UnityEngine.EventSystems;

namespace XPlan.UI
{
    public class DDItemViewBase<TItemViewModel> : ItemViewBase<TItemViewModel>
        , IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerEnterHandler, IPointerExitHandler
        where TItemViewModel : DDItemViewModelBase
    {
        // 快取好的委派（避免每次事件都反射 Invoke）
        private Action<TItemViewModel, PointerEventData> _onBeginDrag;
        private Action<TItemViewModel, PointerEventData> _onDrag;
        private Action<TItemViewModel, PointerEventData> _onEndDrag;
        private Action<TItemViewModel, PointerEventData> _onDrop;

        private Action<TItemViewModel, PointerEventData> _onItemEnter;
        private Action<TItemViewModel, PointerEventData> _onItemExit;

        // ===============================
        // Event Trigger
        // ===============================
        public void OnBeginDrag(PointerEventData eventData)
        {
            _onBeginDrag?.Invoke(_viewModel, eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            _onDrag?.Invoke(_viewModel, eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _onEndDrag?.Invoke(_viewModel, eventData);
        }

        public void OnDrop(PointerEventData eventData)
        {
            _onDrop?.Invoke(_viewModel, eventData);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _onItemEnter?.Invoke(_viewModel, eventData);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _onItemExit?.Invoke(_viewModel, eventData);
        }
    }
}
