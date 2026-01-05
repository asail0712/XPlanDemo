using XPlan.UI;

namespace Demo.Inventory
{
    public class InventoryView : DDViewBase<InventoryViewModel, InventoryItemView, InventoryItemViewModel>
    {
        protected override void OnTableViewReady(InventoryViewModel vm)
        {
            base.OnTableViewReady(vm);

            ImageGhostIconController iconController = GetComponent<ImageGhostIconController>();

            vm.SetGhostController(iconController);
        }
    }
}
