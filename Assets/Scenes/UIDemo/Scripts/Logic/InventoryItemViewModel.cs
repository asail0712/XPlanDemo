using XPlan;

namespace Demo.Inventory
{
    public enum InventoryItemType
    {
        Nothing,
        CannotDrag,
        WarningDrag,
    }

    public class InventoryItemViewModel : DDItemViewModelBase
    {
        public ObservableProperty<InventoryItemData> ItemData = new ObservableProperty<InventoryItemData>();
        public ObservableProperty<InventoryItemType> ItemType = new ObservableProperty<InventoryItemType>(InventoryItemType.Nothing);

        public void SetType(InventoryItemType type)
        {
            ItemType.Value = type;
        }

        public override bool CanDrag()
        {
            return !IsEmpty();
        }

        public bool IsEmpty()
        {
            return ItemData.Value == null || ItemData.Value.IsEmpty();
        }

        public override IGhostPayload CreateGhostPayload()
        {
            if(ItemData.Value == null || ItemData.Value.IsEmpty())
            {
                return null;
            }

            return new GhostSpritePayload(ItemData.Value.IconKey);
        }
    }

    public sealed class InventoryItemData
    {
        public string IconKey { get; set; } = string.Empty;
        public float Alpha { get; set; }
        public bool Mask { get; set; }
        public int ItemId { get; set; }     = -1;

        public bool IsEmpty()
        {
            return string.IsNullOrEmpty(IconKey) && ItemId == -1;
        }
    }
}
