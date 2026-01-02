using UnityEngine;
using UnityEngine.UI;

using XPlan.UI;

using Demo;

namespace Demo.Table
{
    public class TableItemView : ItemViewBase<TableItemViewModel>
    {
        [SerializeField]
        private Text descTxt;

        [SerializeField]
        private Button cloneBtn;

        [SerializeField]
        private Button deleteBtn;
    }
}
