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
using System.Collections.Generic;
using XPlan.Utility;

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

        protected void InsertData(TItemViewModel newItem, int i)
        {
            // ... 執行資料清理或轉換邏輯 ...
            Items.Value.Insert(i, newItem);
            Items.ForceNotify();    // 強制觸發 Items 的 OnValueChanged 事件

            // 同步更新其他屬性
            IsListRootVisible.Value = Items.Value != null && Items.Value.Count != 0;
        }
        protected TItemViewModel GetData(int index)
        {
            if(!Items.Value.IsValidIndex(index))
            {
                return null; //default(TItemViewModel);
            }

            return Items.Value[index];
        }

        protected List<TItemViewModel> GetAll()
        {
            return Items.Value;
        }

        protected void ModifyData(TItemViewModel newItem, int index)
        {
            if (!Items.Value.IsValidIndex(index))
            {
                return;
            }

            Items.Value[index] = newItem;
            Items.ForceNotify();    // 強制觸發 Items 的 OnValueChanged 事件
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

        protected void RemoveData(TItemViewModel item)
        {
            // ... 執行資料清理或轉換邏輯 ...
            Items.Value.Remove(item);   
            Items.ForceNotify();        // 強制觸發 Items 的 OnValueChanged 事件

            // 同步更新其他屬性
            IsListRootVisible.Value = Items.Value != null && Items.Value.Count != 0;
        }
    }
}