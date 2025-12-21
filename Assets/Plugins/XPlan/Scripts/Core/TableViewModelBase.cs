using System.Collections.Generic;

namespace XPlan
{
    // TItemViewModel 必須是 ItemViewModelBase 的子類
    public class TableViewModelBase<TItemViewModel> : ViewModelBase
        where TItemViewModel : ItemViewModelBase
    {
        // 核心：列表中的資料集合
        // ObservableCollection/ReactiveCollection 類別，此處暫用 ObservableProperty 包裝 List。
        internal ObservableProperty<List<TItemViewModel>> Items { get; }  = new(new List<TItemViewModel>());
        internal ObservableProperty<bool> IsListRootVisible { get; }      = new(false);

        // 定義一個方法來更新 Items 屬性
        protected void AddFirst(TItemViewModel newItem)
        {
            // ... 執行資料清理或轉換邏輯 ...
            Items.Value.Insert(0, newItem);
            Items.ForceNotify();    // 強制觸發 Items 的 OnValueChanged 事件

            // 同步更新其他屬性
            IsListRootVisible.Value = Items.Value != null && Items.Value.Count != 0;
        }

        protected void AddData(TItemViewModel newItem)
        {
            // ... 執行資料清理或轉換邏輯 ...
            Items.Value.Add(newItem);
            Items.ForceNotify();    // 強制觸發 Items 的 OnValueChanged 事件

            // 同步更新其他屬性
            IsListRootVisible.Value = Items.Value != null && Items.Value.Count != 0;
        }

        protected void ClearData()
        {
            // ... 執行資料清理或轉換邏輯 ...
            Items.Value.Clear(); 
            Items.ForceNotify();    // 強制觸發 Items 的 OnValueChanged 事件

            // 同步更新其他屬性
            IsListRootVisible.Value = Items.Value != null && Items.Value.Count != 0;
        }


        protected void LoadData(List<TItemViewModel> newItems)
        {
            // ... 執行資料清理或轉換邏輯 ...
            Items.Value             = newItems; // 賦值會觸發 Items 的 OnValueChanged 事件

            // 同步更新其他屬性
            IsListRootVisible.Value = Items.Value != null && Items.Value.Count != 0;
        }
    }
}