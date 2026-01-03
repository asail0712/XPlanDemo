// ==============================================================================
// XPlan Framework
//
// Copyright (c) 2026 Asail
// All rights reserved.
//
// Author  : Asail0712
// Project : XPlan
// Description:
//     A modular framework for Unity projects, focusing on MVVM architecture,
//     runtime tooling, event-driven design, and extensibility.
//
// Contact : asail0712@gmail.com
// GitHub  : https://github.com/asail0712/XPlanDemo
//
// Unauthorized copying, modification, or distribution of this file,
// via any medium, is strictly prohibited without prior permission.
// ==============================================================================
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

using XPlan.Recycle;
using XPlan.Utility;

namespace XPlan.UI
{
    // TTableViewModel 必須是 TableViewModelBase 的子類
    // ItemViewType 是實際的 Item View 類別，它必須繼承 ItemViewBase
    // TItemViewModel 是 Item View 所綁定的資料模型
    public class TableViewBase<TTableViewModel, TItemView, TItemViewModel> : ViewBase<TTableViewModel>, IViewModelGetter<TTableViewModel>
        where TTableViewModel : TableViewModelBase<TItemViewModel>
        where TItemView : ItemViewBase<TItemViewModel>, IPoolable, new()
        where TItemViewModel : ItemViewModelBase
    {
        [Header("TableView")]
        // 列表特有的序列化欄位
        [SerializeField]
        protected GameObject _listRoot; // Unity UI Content
        [SerializeField]
        protected TItemView _itemViewPrefab;   // 列表單元 Prefab

        [Header("Recycle Pool")]
        [SerializeField] 
        private int _prewarmCount = 20;
        [SerializeField] 
        private Transform _poolRoot;   // 指定一個 PoolRoot，避免 LayoutGroup 計算到回收物件

        // 列表 Items 的實體管理
        protected readonly Dictionary<TItemViewModel, TItemView> _activeItemViews   = new();

        // 解除訂閱集中管理
        private readonly List<IDisposable> _disposables                             = new(); 

        // Table View Model
        private TTableViewModel _viewModel;

        public void OnViewModelReady(TTableViewModel vm)
        {
            _viewModel = vm;

            if (vm == null) return;

            // Pool 註冊（只做一次）
            TryRegisterPoolOnce();

            // 手動訂閱root Visible設定
            var disp = vm.IsListRootVisible.Subscribe(isVisible =>
            {
                if (_listRoot != null)
                {
                    _listRoot.SetActive(isVisible);
                }
            });

            // 將訂閱加入 Disposables 列表以便在 OnDestroy 時清理
            _disposables.Add(disp);

            // TableView 專屬的 Item 集合綁定
            BindItemCollection(vm.Items);
        }

        private void TryRegisterPoolOnce()
        {
            if (_itemViewPrefab == null) return;

            // 如果沒填 poolRoot，就自動建一個（可選）
            if (_poolRoot == null)
            {
                var go = new GameObject($"{typeof(TItemView).Name}_PoolRoot");
                go.transform.SetParent(transform, false);
                _poolRoot = go.transform;
            }

            // ✅ 用 prefab.gameObject 註冊
            RecyclePool<TItemView>.RegisterType(
                _itemViewPrefab.gameObject,
                _prewarmCount,
                _poolRoot.gameObject
            );
        }

        protected virtual void OnTableViewReady(TTableViewModel vm) { /* 子類實作 */ }

        /// <summary>
        /// 訂閱 Item 集合的變動並處理 UI 渲染。
        /// </summary>
        private void BindItemCollection(ObservableProperty<List<TItemViewModel>> itemsProp)
        {
            // 訂閱集合變動事件，並將訂閱器加入 _disposables 統一管理
            var disp = itemsProp.Subscribe(RenderItems);
            _disposables.Add(disp);

            // 推送初值
            itemsProp.ForceNotify();
        }

        /// <summary>
        /// 根據新的 ViewModel 列表渲染 UI 列表（簡單的全更新/非物件池實作）
        /// </summary>
        private void RenderItems(List<TItemViewModel> newItems)
        {
            if (_itemViewPrefab == null || _listRoot == null)
            {
                Debug.LogError("[TableViewBase] Prefab 或 Content Container 尚未設置！");
                return;
            }

            newItems ??= new List<TItemViewModel>();

            // 用 HashSet 快速判斷哪些要移除
            var newSet = new HashSet<TItemViewModel>(newItems);

            // 1) 只回收「不在新清單」的舊 view（其他全部留在場景）
            var toRemove = new List<TItemViewModel>();
            foreach (var kv in _activeItemViews)
            {
                if (!newSet.Contains(kv.Key))
                    toRemove.Add(kv.Key);
            }

            foreach (var key in toRemove)
            {
                var view = _activeItemViews[key];
                if (view != null)
                    RecyclePool<TItemView>.Recycle(view);

                _activeItemViews.Remove(key);
            }

            // 2) 依序確保每個 item 都有 view；有就沿用，沒有才 Spawn
            for (int i = 0; i < newItems.Count; i++)
            {
                var itemVM = newItems[i];

                if (!_activeItemViews.TryGetValue(itemVM, out var itemView) || itemView == null)
                {
                    itemView = RecyclePool<TItemView>.SpawnOne();
                    _listRoot.AddChild(itemView.gameObject);

                    itemView.SetViewModel(itemVM);

                    // 只在「新建」時做事件綁定，避免每次刷新 RemoveAllListeners 造成抖動
                    AutoBindItemViewEvents(itemView, itemVM);

                    _activeItemViews[itemVM] = itemView;
                }
                else
                {
                    // 留在場景：只要確保 parent 正確
                    if (itemView.transform.parent != _listRoot.transform)
                        _listRoot.AddChild(itemView.gameObject);
                }

                // 3) 排序變了就調 sibling，不移除不重建
                itemView.transform.SetSiblingIndex(i);
            }

            OnTableViewReady(_viewModel);
        }

        private new void OnDestroy()
        {
            base.OnDestroy();

            // 複製 ViewBase 的資源釋放
            foreach (var d in _disposables) d?.Dispose();
            _disposables.Clear();

            // 清理 Item View 的實例
            foreach (var kv in _activeItemViews)
            {
                if (kv.Value != null) Destroy(kv.Value.gameObject);
            }
            _activeItemViews.Clear();

            RecyclePool<TItemView>.UnregisterType();
        }

        /// <summary>
        /// 使用反射自動綁定 ItemView 上的按鈕到 TableViewModel 中的對應方法。
        /// 約定：ItemView 的按鈕 Field 名稱為 XxxBtn，TableViewModel 的方法名稱為 OnXxxClick(TItemViewModel)。
        /// </summary>
        private void AutoBindItemViewEvents(TItemView itemView, TItemViewModel itemVM)
        {
            // 查找 ItemView 中所有 UnityEngine.UI.Button 型別的公開或非公開欄位
            var itemFieldInfos = typeof(TItemView).GetFields(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var fieldInfo in itemFieldInfos)
            {
                if (fieldInfo.FieldType == typeof(Button))
                {
                    // 1. 根據欄位名稱，推斷 TableViewModel 中目標方法名稱
                    string baseName     = ViewBindingHelper.DeriveBaseName(fieldInfo);
                    string methodName   = $"On{baseName}Click";

                    // 2. 查找 TableViewModel 中的目標方法
                    var methodInfo      = typeof(TTableViewModel).GetMethod(methodName,
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                    // 3. 檢查方法是否存在且簽名正確以及加上正確的attribute
                    if (methodInfo == null || !methodInfo.IsDefined(typeof(ButtonBindingAttribute), inherit: true))
                    {
                        continue; // 找不到對應方法則跳過
                    }

                    var parameters      = methodInfo.GetParameters();
                    // 檢查方法簽名是否為：void Method(TItemViewModel vm)
                    if (parameters.Length != 1 || parameters[0].ParameterType != typeof(TItemViewModel) || methodInfo.ReturnType != typeof(void))
                    {
                        LogSystem.Record($"[TableView] 方法簽名錯誤: {methodName} 必須是 void {methodName}({typeof(TItemViewModel).Name})", LogType.Error);
                        continue;
                    }

                    // 4. 取得 ItemView 中的 Button 實例
                    Button button = fieldInfo.GetValue(itemView) as Button;
                    if (button == null) continue;

                    // 4.5 池物件會重用，先清掉舊綁定，避免累加與舊 closure
                    button.onClick.RemoveAllListeners();

                    // 5. 創建 Action 委派，將 ItemViewModel 作為參數傳遞
                    // 使用匿名函數 (Closure) 來捕獲 itemVM，並呼叫反射找到的方法
                    Action clickAction = () =>
                    {
                        // 執行 TableViewModel 上的方法，傳入 ItemViewModel 作為參數
                        try
                        {
                            methodInfo.Invoke(_viewModel, new object[] { itemVM });
                        }
                        catch (Exception ex)
                        {
                            LogSystem.Record($"[TableView] 執行 {methodName} 失敗: {ex}", LogType.Error);
                        }
                    };

                    // 6. 將 Action 綁定到 Unity Button
                    button.onClick.AddListener(() => clickAction.Invoke());

                    // 【重要】記錄綁定以便 OnDestroy 時清理（需要擴展 _disposables 或新增 map）
                    // 這裡簡化處理，因為 Button.onClick.RemoveAllListeners 會在 ItemView 銷毀時處理。
                    // 如果需要更精確的清理，可以考慮在 ItemViewBase 裡新增一個 IDisposable 清理此綁定。

                    LogSystem.Record($"[TableView] 成功綁定: {fieldInfo.Name} -> {methodName}");
                }
            }
        }
    }
}