using System.Collections;
using UnityEngine;

using XPlan;
using XPlan.UI;
using XPlan.Utility;

namespace Demo
{
    public class TableItemViewModel : ItemViewModelBase
    {
        public ObservableProperty<string> desc = new ObservableProperty<string>();

        public TableItemViewModel(string descStr) 
        {
            desc.Value = descStr;
        }
    }
}