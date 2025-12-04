// 檔案: ItemViewBase.cs
using System;
using System.Collections.Generic;
using UnityEngine;

namespace XPlan.UI
{
    // TItemViewModel 必須是 ItemViewModelBase 的子類
    public class ItemViewBase<TItemViewModel> : MonoBehaviour
        where TItemViewModel : ItemViewModelBase
    {
        protected TItemViewModel _viewModel;
        private readonly List<IDisposable> _disposables = new();    // Item View 內部的訂閱列表
        private readonly SpriteCache _spriteCache       = new();    // 每個 Item View 使用自己的 SpriteCache 以供 Image 綁定

        /// <summary>
        /// 由 TableView 呼叫，設定此單元的 ViewModel 並執行自動綁定。
        /// </summary>
        public void SetViewModel(TItemViewModel vm)
        {
            // 清理舊的訂閱
            foreach (var d in _disposables) d?.Dispose();
            _disposables.Clear();
            _spriteCache.Dispose(); // 清理舊的 Sprite Cache

            _viewModel = vm;

            if (vm == null) return;

            // VM → UI 綁定：利用 ViewBindingHelper
            // Item View 通常只處理 VM→UI 綁定，不需要 UI→VM 的 AutoRegisterComponents
            ViewBindingHelper.AutoBindObservables(
                this,
                vm,
                _disposables,
                _spriteCache,
                OnRefreshItem);

            OnDataBound();
        }

        protected virtual void OnRefreshItem()
        {

        }

        protected virtual void OnDataBound()
        {
            // 留給子類別實作，在 ViewModel 綁定和 UI 初始化完成後執行客製化邏輯
        }

        private void OnDestroy()
        {
            // Item View 被銷毀時，確保所有訂閱和快取被釋放
            foreach (var d in _disposables)
            {
                d?.Dispose();
            }
            _disposables.Clear();
            _spriteCache.Dispose();
        }
    }
}