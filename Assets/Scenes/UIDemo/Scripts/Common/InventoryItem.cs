using UnityEngine;

namespace Demo.Inventory
{
    public class InventoryItem
    {
        public string id;     

        public Sprite Icon;

        public InventoryItem(string id, Sprite icon)
        {
            this.id = id;
            Icon    = icon;
        }
    }
}
