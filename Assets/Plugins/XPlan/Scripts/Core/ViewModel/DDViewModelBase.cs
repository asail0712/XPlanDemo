// ==============================================================================
// XPlan Framework
//
// Copyright (c) 2026 Asail
// All rights reserved.
//
// Author  : Asail0712
// Project : XPlan
// Description:
//     A modular framework for Unity projects, focusing on MVVM architecture,
//     runtime tooling, event-driven design, and extensibility.
//
// Contact : asail0712@gmail.com
// GitHub  : https://github.com/asail0712/XPlanDemo
//
// Unauthorized copying, modification, or distribution of this file,
// via any medium, is strictly prohibited without prior permission.
// ==============================================================================
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
            if (!itemVM.CanDrag())
                return;

            if (_ctx != null)
                CancelDrag("Begin while previous ctx still alive");

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
            _ctx.Phase          = DragPhase.Drop;
            _ctx.DropTarget     = itemVM;
            DragOutcome outcome = OnDragDrop(_ctx);

            // 強制觸發 end 避免有時候OnEndDrag 無法觸發到
            _ctx.Phase          = DragPhase.End;
            OnDragEnd(_ctx);
            EndDragAndCleanup(outcome);
        }

        [DragBinding(DragPhase.DragEnter)]
        public void OnDragEnter(TDDItemViewModel itemVM, PointerEventData e)
        {
            if (_ctx == null) return;
            if (e.pointerId != _ctx.PointerId) { CancelDrag("Pointer mismatch on Drop"); return; }

            _ctx.DragHoverItem  = itemVM;

            OnDragEnter(_ctx);
        }

        [DragBinding(DragPhase.DragExit)]
        public void OnDragExit(TDDItemViewModel itemVM, PointerEventData e)
        {
            if (_ctx == null) return;
            if (e.pointerId != _ctx.PointerId) { CancelDrag("Pointer mismatch on Drop"); return; }

            OnDragExit(_ctx);

            _ctx.DragHoverItem = null;
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

            // Drop 與 EndDrag透過 _ctx == null 來避免做兩次clean
            // 若是延後_ctx = null，可能會造成兩次clean
            var tmpCtx  = _ctx;
            _ctx        = null;

            if (dragOutcome == DragOutcome.RejectSnapBack)
                await SnapBack(tmpCtx);

            if (hideGhost)
                _ghost?.Hide();
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
        protected virtual void OnDragEnter(DragContext<TDDItemViewModel> ctx)
        {
            // for override            
        }
        protected virtual void OnDragExit(DragContext<TDDItemViewModel> ctx)
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
        public TDDItemViewModel DropTarget { get; set; }
        public TDDItemViewModel DragHoverItem { get; set; }

        // position
        public Vector2 StartScreenPos { get; set; }
        public Vector2 CurrentScreenPos { get; set; }
        public Vector2 Delta => CurrentScreenPos - StartScreenPos;

        // state
        public int PointerId { get; set; }
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

        DragEnter,
        DragExit,
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
