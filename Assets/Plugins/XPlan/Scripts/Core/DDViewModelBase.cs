using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

namespace XPlan
{
    public class DDViewModelBase<TDDItemViewModel> : TableViewModelBase<TDDItemViewModel>
         where TDDItemViewModel : DDItemViewModelBase, new()
    {
        private DragContext<TDDItemViewModel> _ctx  = null;
        private TDDItemViewModel _currHoverItemVM   = null;
        private IGhostIconController _ghost         = null;

        public void InitTable(int num)
        {
            List<TDDItemViewModel> itemVMList = new List<TDDItemViewModel>();

            for (int i = 0; i < num; ++i)
            {
                TDDItemViewModel itemVM = new TDDItemViewModel();
                itemVMList.Add(itemVM);
            }

            LoadData(itemVMList);
        }

        /********************************
         * for Drag
         * *****************************/

        [DragBinding(DragPhase.Begin)]
        public void OnBeginDrag(TDDItemViewModel itemVM, PointerEventData e)
        {
            if (_ctx != null)
                CancelDrag("Begin while previous ctx still alive");

            ClearHover();

            // drag
            _ctx = new DragContext<TDDItemViewModel>
            {
                SourceItem          = itemVM,
                PointerId           = e.pointerId,
                StartScreenPos      = e.position,
                CurrentScreenPos    = e.position,
                Phase               = DragPhase.Begin
            };

            OnDragBegin(_ctx);

            IGhostPayload ghostPayload = itemVM.CreateGhostPayload();

            if(ghostPayload != null)
            {
                // ghost icon
                _ghost?.Bind(ghostPayload);
                _ghost?.Show(e.position);
                _ghost?.Move(_ctx.CurrentScreenPos);
            }
        }

        [DragBinding(DragPhase.Drag)]
        public void OnDrag(TDDItemViewModel itemVM, PointerEventData e)
        {
            if (_ctx == null) return;
            if (e.pointerId != _ctx.PointerId) { CancelDrag("Pointer mismatch on Drag"); return; }

            // drag
            _ctx.CurrentScreenPos   = e.position;
            _ctx.Phase              = DragPhase.Drag;

            OnDragUpdate(_ctx);

            // ghost icon
            _ghost?.Move(_ctx.CurrentScreenPos);
        }

        [DragBinding(DragPhase.End)]
        public void OnEndDrag(TDDItemViewModel itemVM, PointerEventData e)
        {
            if (_ctx == null) return;
            if (e.pointerId != _ctx.PointerId) { CancelDrag("Pointer mismatch on End"); return; }

            // drag
            _ctx.Phase = DragPhase.End;

            OnDragEnd(_ctx);
            EndDragAndCleanup(DragOutcome.RejectSnapBack);
        }

        [DragBinding(DragPhase.Drop)]
        public void OnDrop(TDDItemViewModel itemVM, PointerEventData e)
        {
            if (_ctx == null) return;
            if (e.pointerId != _ctx.PointerId) { CancelDrag("Pointer mismatch on Drop"); return; }

            // drag
            _ctx.Phase      = DragPhase.Drop;
            _ctx.DropTarget = itemVM;

            EndDragAndCleanup(OnDragDrop(_ctx));
        }

        [DragBinding(DragPhase.ItemEnter)]
        public void OnItemEnter(TDDItemViewModel itemVM, PointerEventData e)
        {
            // 只處理「hover 真的變更」的情況
            if (_currHoverItemVM != null && EqualityComparer<TDDItemViewModel>.Default.Equals(_currHoverItemVM, itemVM))
                return;

            var oldHover        = _currHoverItemVM;
            _currHoverItemVM    = itemVM;

            OnHoverChanged(oldHover, _currHoverItemVM, e, _ctx != null);
        }

        [DragBinding(DragPhase.ItemExit)]
        public void OnItemExit(TDDItemViewModel itemVM, PointerEventData e)
        {
            // 只處理「退出的就是當前 hover」的情況
            if (_currHoverItemVM != null && !EqualityComparer<TDDItemViewModel>.Default.Equals(_currHoverItemVM, itemVM))
                return;

            var oldHover        = _currHoverItemVM;
            _currHoverItemVM    = null;

            OnHoverChanged(oldHover, _currHoverItemVM, e, _ctx != null);
        }

        private void CancelDrag(string reason)
        {
            if (_ctx == null)
                return;

            // drag
            _ctx.Phase = DragPhase.Cancel;

            // UI cleanup only            
            EndDragAndCleanup(DragOutcome.RejectSnapBack);
        }

        private async void EndDragAndCleanup(DragOutcome dragOutcome, bool hideGhost = true)
        {
            if (_ctx == null) 
                return; // 已結算/已清理（例如 Drop 已處理或物件狀態變更）

            if (dragOutcome == DragOutcome.RejectSnapBack)
                await SnapBack(_ctx);

            if (hideGhost)
                _ghost?.Hide();

            ClearHover();

            _ctx = null;
        }

        /********************************
         * for Hover
         * *****************************/
        private void ClearHover()
        {
            if (_currHoverItemVM == null) return;

            var oldHover        = _currHoverItemVM;
            _currHoverItemVM    = null;

            OnHoverChanged(oldHover, _currHoverItemVM, null, _ctx != null);
        }

        /********************************
         * for Ghost Icon
         * *****************************/
        public void SetGhostController(IGhostIconController ghost)
        {
            // 先把舊的關掉，避免殘留
            _ghost?.Hide();
            _ghost = ghost;

            // 如果正在拖曳，立刻同步狀態（可選）
            if (_ctx != null && _ctx.IsDragging)
            {
                _ghost?.Show(_ctx.CurrentScreenPos);
                _ghost?.Move(_ctx.CurrentScreenPos);
            }
        }

        /********************************
         * for SnapBack
         * *****************************/
        private async Task SnapBack(DragContext<TDDItemViewModel> ctx)
        {
            await _ghost?.SnapBackTo(ctx.StartScreenPos);

            OnSnapBack(ctx);
        }

        /********************************
         * for 衍生類別 覆寫
         * *****************************/
        protected virtual void OnDragBegin(DragContext<TDDItemViewModel> ctx) 
        {
            // for override
        }
        protected virtual void OnDragUpdate(DragContext<TDDItemViewModel> ctx) 
        {
            // for override
        }
        protected virtual void OnDragEnd(DragContext<TDDItemViewModel> ctx) 
        {
            // for override
        }
        protected virtual DragOutcome OnDragDrop(DragContext<TDDItemViewModel> ctx) 
        {
            // for override
            return DragOutcome.RejectSnapBack;
        }
        protected virtual void OnHoverChanged(TDDItemViewModel oldHover, TDDItemViewModel newHover, PointerEventData e, bool duringDrag)
        {
            // for override
        }

        protected virtual void OnSnapBack(DragContext<TDDItemViewModel> ctx)
        {

        }
    }

    public sealed class DragContext<TDDItemViewModel>
    {
        // source
        public TDDItemViewModel SourceItem { get; set; }
        public int PointerId { get; set; }

        // position
        public Vector2 StartScreenPos { get; set; }
        public Vector2 CurrentScreenPos { get; set; }
        public Vector2 Delta => CurrentScreenPos - StartScreenPos;

        // state
        public TDDItemViewModel DropTarget { get; set; }
        public DragPhase Phase { get; set; }
        public bool IsDragging => Phase == DragPhase.Begin || Phase == DragPhase.Drag;
        public bool IsDropped => Phase == DragPhase.Drop;
        public bool IsCancelled => Phase == DragPhase.Cancel;
    }
    public enum DragPhase
    {
        Begin,
        Drag,
        End,
        Drop,

        Cancel,

        ItemEnter,
        ItemExit,
    }

    public interface IGhostIconController
    {
        void Show(Vector2 screenPos);
        void Move(Vector2 screenPos);
        void Hide();
        Task SnapBackTo(Vector2 startScreenPos); // optional
        void Bind(IGhostPayload payload);   // 或 Bind(IGhostVisualData data)
    }

    public enum DragOutcome
    {
        Accept,          // 成功，不回彈
        RejectSnapBack,  // 失敗，要回彈
        RejectNoSnapBack // 失敗，但不回彈（例如丟到地板生成掉落物）
    }
}
