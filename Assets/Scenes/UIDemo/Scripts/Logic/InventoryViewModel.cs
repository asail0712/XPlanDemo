using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using XPlan;

namespace Demo.Inventory
{
    public class InventoryViewModel : DDViewModelBase<InventoryItemViewModel>
    {
        private const int NumOfSlot = 30;
        public InventoryViewModel()
        {
            InitTable(NumOfSlot);

            RandomItem();
            RandomItem();
            RandomItem();
        }

        protected override void OnDragBegin(DragContext<InventoryItemViewModel> ctx)
        {
            List<InventoryItemViewModel> vmList = GetAll();

            foreach(InventoryItemViewModel vm in vmList)
            {
                vm.SetType(vm.IsEmpty() ? InventoryItemType.Nothing : InventoryItemType.CannotDrag);
            }
        }
        protected override void OnDragUpdate(DragContext<InventoryItemViewModel> ctx)
        {
            // for override
        }
        protected override void OnDragEnd(DragContext<InventoryItemViewModel> ctx)
        {
            List<InventoryItemViewModel> vmList = GetAll();

            foreach (InventoryItemViewModel vm in vmList)
            {
                vm.SetType(InventoryItemType.Nothing);
            }
        }
        protected override DragOutcome OnDragDrop(DragContext<InventoryItemViewModel> ctx)
        {
            if(ctx.SourceItem.IsEmpty())
            {
                return DragOutcome.RejectNoSnapBack;
            }

            if(ctx.DropTarget.IsEmpty())
            {
                ctx.DropTarget.ItemData.Value = ctx.SourceItem.ItemData.Value;
                ctx.SourceItem.ItemData.Value = null;

                return DragOutcome.Accept;
            }
            else
            {
                return DragOutcome.RejectSnapBack;
            }                
        }
        protected override void OnDragEnter(DragContext<InventoryItemViewModel> ctx)
        {
            if (ctx.DragHoverItem == null)
                return;

            if (!ctx.DragHoverItem.IsEmpty() && ctx.DragHoverItem != ctx.SourceItem)
                ctx.DragHoverItem.SetType(InventoryItemType.WarningDrag);
        }
        protected override void OnDragExit(DragContext<InventoryItemViewModel> ctx)
        {
            if (ctx.DragHoverItem == null)
                return;

            if (!ctx.DragHoverItem.IsEmpty())
                ctx.DragHoverItem.SetType(InventoryItemType.CannotDrag);
        }

        protected override void OnSnapBack(DragContext<InventoryItemViewModel> ctx)
        {

        }

        static int id = 0;

        private void RandomItem()
        {
            List<InventoryItemViewModel> vmList = GetAll();
            List<int> emptyList                 = new List<int>();

            for(int i = 0; i < vmList.Count; ++i)
            {
                InventoryItemViewModel itemVM = vmList[i];

                if(itemVM.IsEmpty())
                {
                    emptyList.Add(i);
                }
            }

            if(emptyList.Count == 0)
            {
                return;
            }

            int randEmpty               = emptyList[Random.Range(0, emptyList.Count)];

            InventoryItemData itemData  = new InventoryItemData();
            itemData.ItemId             = id++;
            itemData.IconKey            = "I_Potion";

            InventoryItemViewModel vm   = new InventoryItemViewModel();
            vm.ItemData.Value           = itemData;

            ModifyData(vm, randEmpty);
        }
    }
}