using XPlan;

namespace Demo.Inventory
{
    public class InventorySystem : SystemBase
    {
        protected override void OnInitialLogic()
        {
            InventoryViewModel vm = new InventoryViewModel();

            RegisterLogic(vm);
        }   
    }
}
