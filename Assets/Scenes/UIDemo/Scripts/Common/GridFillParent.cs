using UnityEngine;
using UnityEngine.UI;

namespace Demo.Inventory
{
    [ExecuteAlways]
    public class GridFillParent : MonoBehaviour
    {
        [SerializeField] private int columns    = 5;   // 固定欄數
        [SerializeField] private int rows       = 5;   // 固定列數

        private GridLayoutGroup grid;
        private RectTransform rt;

        void OnEnable()
        {
            grid    = GetComponent<GridLayoutGroup>();
            rt      = GetComponent<RectTransform>();

            Apply();
        }

        void OnRectTransformDimensionsChange() => Apply();
        void OnValidate() => Apply();

        void Apply()
        {
            if (!grid || !rt) return;
            if (columns <= 0 || rows <= 0) return;

            var rect = rt.rect.size;

            // GridLayoutGroup 的 padding + spacing
            float totalPadX     = grid.padding.left + grid.padding.right;
            float totalPadY     = grid.padding.top + grid.padding.bottom;

            float totalSpaceX   = grid.spacing.x * (columns - 1);
            float totalSpaceY   = grid.spacing.y * (rows - 1);

            float cellW         = (rect.x - totalPadX - totalSpaceX) / columns;
            float cellH         = (rect.y - totalPadY - totalSpaceY) / rows;

            // 需要正方形格子就取 min
            float cell          = Mathf.Floor(Mathf.Min(cellW, cellH));
            grid.cellSize       = new Vector2(cell, cell);
        }
    }
}
