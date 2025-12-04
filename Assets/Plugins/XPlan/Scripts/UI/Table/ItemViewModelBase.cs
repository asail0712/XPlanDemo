// 檔案: ItemViewModelBase.cs
using XPlan.UI;

namespace XPlan.UI
{
    // 作為列表中單一項目 ViewModel 的基底。
    // 繼承 ViewModelBase 以便使用 ObservableProperty 的自動通知機制。
    public class ItemViewModelBase : ViewModelBase
    {
        // 可以新增一些通用的 Item 屬性，例如：
        // public ObservableProperty<bool> IsSelected { get; } = new();

        // **重要：ItemViewModelBase 不應呼叫 VMLocator.Register(this)**
        public ItemViewModelBase()
            : base(false)
        {
            
        }
    }
}