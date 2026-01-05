using XPlan;
using XPlan.Utility;

namespace Demo.Inventory
{
    public class InventorySystem : SystemBase
    {
        protected override void OnPreInitial()
        {
            GameViewSizeForce.EnsureAndUseFixed("XPlan.Demo", 1440, 2960);
        }

        protected override void OnInitialLogic()
        {
            InventoryViewModel vm = new InventoryViewModel();

            RegisterLogic(vm);
        }   
    }
}
