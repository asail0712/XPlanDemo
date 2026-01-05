using UnityEngine;

using XPlan;
using XPlan.UI;
using XPlan.Utility;

namespace Demo
{
    // 系統層的進入點，負責初始注入邏輯與提供測試按鍵（ContextMenu）操作
    public class TableSystem : SystemBase
    {
        protected override void OnPreInitial()
        {
            Application.targetFrameRate = 60;
            GameViewSizeForce.EnsureAndUseFixed("XPlan.Demo", 1440, 2960);
        }

        protected override void OnInitialLogic()
        {
            RegisterLogic(new TableViewModel());
        }

        /*****************************************
         * 在 Inspector 右鍵選單中 建立以下功能
         * **************************************/
        [ContextMenu("增加一個Item")]
        private void AddItem()
        {
            new AddItemDescMsg().Send();
        }
    }
}