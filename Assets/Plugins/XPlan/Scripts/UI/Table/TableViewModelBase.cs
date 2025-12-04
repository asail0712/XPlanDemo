// 檔案: TableViewModelBase.cs
using System.Collections.Generic;

namespace XPlan.UI
{
    // TItemViewModel 必須是 ItemViewModelBase 的子類
    public class TableViewModelBase<TItemViewModel> : ViewModelBase
        where TItemViewModel : ItemViewModelBase
    {
        // 核心：列表中的資料集合
        // 注意：如果您需要集合增減/排序時發出通知，您可能需要一個更進階的
        // ObservableCollection/ReactiveCollection 類別，此處暫用 ObservableProperty 包裝 List。
        public ObservableProperty<List<TItemViewModel>> Items { get; }  = new(new List<TItemViewModel>());
        public ObservableProperty<bool> IsListRootVisible { get; }      = new(false);

        // 您可以定義一個方法來更新 Items 屬性
        public void LoadData(List<TItemViewModel> newItems)
        {
            // ... 執行資料清理或轉換邏輯 ...
            Items.Value             = newItems; // 賦值會觸發 Items 的 OnValueChanged 事件

            // 同步更新其他屬性
            IsListRootVisible.Value = newItems == null || newItems.Count == 0;
        }
    }
}