// 檔案: ItemViewBase.cs
using System;
using System.Collections.Generic;
using XPlan.Recycle;

namespace XPlan.UI
{
    // TItemViewModel 必須是 ItemViewModelBase 的子類
    public class ItemViewBase<TItemViewModel> : PoolableComponent
        where TItemViewModel : ItemViewModelBase
    {
        protected TItemViewModel _viewModel;
        private readonly Dictionary<string, ObservableBinding> _vmObservableMap = new(StringComparer.Ordinal);  // 新增：把 VM 內的 ObservableProperty 索引起來（baseName → 綁定資訊）
        protected readonly List<IDisposable> _disposables                       = new();                        // Item View 內部的訂閱列表
        private readonly SpriteCache _spriteCache                               = new();                        // 每個 Item View 使用自己的 SpriteCache 以供 Image 綁定

        /// <summary>
        /// 由 TableView 呼叫，設定此單元的 ViewModel 並執行自動綁定。
        /// </summary>
        public void SetViewModel(TItemViewModel vm)
        {
            // 清理舊的訂閱
            CleanupBindings();

            _viewModel = vm;

            if (vm == null) return;

            ViewBindingHelper.IndexVmObservables(vm, _vmObservableMap);

            // VM → UI 綁定：利用 ViewBindingHelper
            // Item View 通常只處理 VM→UI 綁定，不需要 UI→VM 的 AutoRegisterComponents
            ViewBindingHelper.AutoBindObservables(
                this,
                vm,
                _disposables,
                _spriteCache);

            ViewBindingHelper.AutoBindObservableHandlers(this, _vmObservableMap, _disposables);

            OnDataBound();
        }

        protected virtual void OnDataBound()
        {
            // 留給子類別實作，在 ViewModel 綁定和 UI 初始化完成後執行客製化邏輯
        }

        // ===============================
        // Poolable lifecycle
        // ===============================

        public override void OnRecycle()
        {
            base.OnRecycle();

            // ✅ 關鍵：回收到池時，也要清理 VM 狀態
            CleanupBindings();
            _viewModel = null;
        }

        protected new void OnDestroy()
        {
            base.OnDestroy();
            CleanupBindings();
        }

        // ===============================
        // Internal
        // ===============================

        private void CleanupBindings()
        {
            foreach (var d in _disposables)
                d?.Dispose();
            _disposables.Clear();

            _spriteCache.Dispose();
            _vmObservableMap.Clear();
        }
    }
}