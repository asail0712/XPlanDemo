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
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using XPlan.Weaver.Runtime;

namespace XPlan.UI
{    
    public sealed class ObservableBinding
    {
        public object OpInstance;       // ObservableProperty<T> 實體
        public Type ValueType;          // T
        public PropertyInfo ValueProp;  // .Value
        public MethodInfo ForceNotify;
    }

    [ViewBinding]
    public class ViewBase<TViewModel> : MonoBehaviour, IUIView where TViewModel : ViewModelBase
    {
        private TViewModel _viewModel;// viewmodel本體
        private IDisposable _waitToken;

        private readonly List<IDisposable> _disposables                         = new();                        // 解除訂閱集中管理
        private readonly Dictionary<string, ObservableBinding> _vmObservableMap = new(StringComparer.Ordinal);  // 新增：把 VM 內的 ObservableProperty 索引起來（baseName → 綁定資訊）
        private readonly SpriteCache _spriteCache                               = new();                        // 給圖片綁定用的 Sprite 快取    
        
        private ButtonBindingHandle _buttonBinding;
        private InputTfBindingHandle _inputTfBinding;
        private ToggleBindingHandle _toggleBindingHandle;

        protected void Awake()
        {
            VMLocator.VMUnregistered += OnVMUnregistered;
            WaitAndBind();
        }
        private void WaitAndBind()
        {
            _waitToken?.Dispose();
            _waitToken = VMLocator.GetOrWait<TViewModel>(BindVM);
        }

        private void BindVM(TViewModel vm)
        {
            // 同一顆就別重綁
            if (_viewModel != null && ReferenceEquals(_viewModel, vm))
                return;

            UnbindAll();

            _viewModel = vm;

            ViewBindingHelper.IndexVmObservables(_viewModel, _vmObservableMap);
            ViewBindingHelper.AutoRegisterComponents(this, _vmObservableMap);
            ViewBindingHelper.AutoBindObservableHandlers(this, _vmObservableMap, _disposables);
            ViewBindingHelper.AutoBindObservables(this, _viewModel, _disposables, _spriteCache);
            ViewBindingHelper.AutoBindVisibility(this, _vmObservableMap, _disposables);

            if (this is IViewModelGetter<TViewModel> getter)
                getter.OnViewModelReady(_viewModel);
        }
        private void OnVMUnregistered(Type t, ViewModelBase deadVm)
        {
            // 只有「死掉的是我綁的那顆」才重等
            if (_viewModel != null && ReferenceEquals(_viewModel, deadVm))
            {
                UnbindAll();
                WaitAndBind();
            }
        }
        private void UnbindAll()
        {
            // 解除 VM→UI 訂閱
            foreach (var d in _disposables) 
                d?.Dispose();
            _disposables.Clear();

            // 清 UI→VM 的索引
            _vmObservableMap.Clear();

            // VM 參考清掉（避免握到舊 VM）
            _viewModel = null;
        }
        protected void OnEnable()
        {
            if (!ViewBindingHelper.TryGetViewModelTypeFromView(this.GetType(), out Type vmType))
                return;

            MethodInfo[] methods = ViewBindingHelper.GetAllInstanceMethods(vmType);

            _buttonBinding          = VmButtonBindingRuntime.Bind(this, methods);
            _inputTfBinding         = VmInputTfBindingRuntime.Bind(this, methods);
            _toggleBindingHandle    = VmToggleBindingRuntime.Bind(this, methods);
        }

        protected void OnDisable()
        {
            VmButtonBindingRuntime.Unbind(_buttonBinding);
            VmInputTfBindingRuntime.Unbind(_inputTfBinding);
            VmToggleBindingRuntime.Unbind(_toggleBindingHandle);
        }

        protected void OnDestroy()
        {
            VMLocator.VMUnregistered -= OnVMUnregistered;

            _waitToken?.Dispose();
            _waitToken = null;

            UnbindAll();

            _spriteCache.Dispose();
        }

        public static async Task<T> WithTimeout<T>(Task<T> task, int timeoutMs)
        {
            using (var cts = new CancellationTokenSource())
            {
                var delay       = Task.Delay(timeoutMs, cts.Token);
                var finished    = await Task.WhenAny(task, delay);

                if (finished == delay)
                    throw new TimeoutException();

                cts.Cancel(); // 終止 delay 任務
                return await task;
            }
        }

        /****************************************
         * 實作IUIView
         * **************************************/        
        public int SortIdx { get; set; }
        
        public void RefreshLanguage(int currLang)
        {
            OnRefreshLanguage(currLang);
        }
        public GameObject GetUIGameObject()
        {
            return this.gameObject;
        }

        public void SetVisibility(bool b)
        {
            if (this.gameObject.activeSelf != b)
                this.gameObject.SetActive(b);
        }

        /****************************************
         * internal virtual methods
         * **************************************/
        protected virtual void OnRefreshLanguage(int currLang)
        {

        }
    }
}
