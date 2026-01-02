using System.Collections;
using System.IO;
using UnityEngine;

using XPlan;
using XPlan.UI;
using XPlan.Utility;

namespace Demo
{
    public class TableViewModel : TableViewModelBase<TableItemViewModel>
    {
        private const int DefaultNum = 5;
        public TableViewModel()
        {
            for(int i = 0; i < DefaultNum; ++i)
            {
                new AddItemDescMsg().Send();
            }
        }

        /****************************
         * Notify Binding
         * *************************/
        [NotifyHandler]
        public void AddItemDesc(AddItemDescMsg msg)
        {
            AddData(new TableItemViewModel(msg.desc));
        }

        /****************************
         * UI Binding
         * *************************/
        [ButtonBinding]
        public void OnCloneClick(TableItemViewModel vm)
        {
            AddData(new TableItemViewModel(vm.desc.Value));
        }

        [ButtonBinding]
        public void OnDeleteClick(TableItemViewModel vm)
        {
            RemoveData(vm);
        }
    }
}