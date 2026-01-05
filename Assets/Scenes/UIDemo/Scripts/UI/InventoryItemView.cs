using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using XPlan;
using XPlan.UI;
using XPlan.Utility;

namespace Demo.Inventory
{
    public class InventoryItemView : DDItemViewBase<InventoryItemViewModel>
    {
        [SerializeField] private Image iconImg;
        [SerializeField] private List<IconInfo> iconList = new List<IconInfo>();

        [ObBinding]
        public void OnItemDataChange(InventoryItemData itemData)
        {
            if (itemData == null || itemData.IsEmpty())
            {
                iconImg.enabled = false;
                return;
            }

            iconImg.enabled = true;

            int idx = iconList.FindIndex(e04 => e04.iconKey == itemData.IconKey);

            if (iconList.IsValidIndex(idx))
            {
                iconImg.sprite = iconList[idx].icon;
            }
        }

        [ObBinding]
        public void OnItemTypeChange(InventoryItemType type)
        {
            switch(type)
            {
                case InventoryItemType.Nothing:
                    iconImg.color = Color.white;
                    break;
                case InventoryItemType.CannotDrag:
                    iconImg.color = Color.gray;
                    break;
                case InventoryItemType.WarningDrag:
                    iconImg.color = Color.red;
                    break;
            }
        }
    }
}
