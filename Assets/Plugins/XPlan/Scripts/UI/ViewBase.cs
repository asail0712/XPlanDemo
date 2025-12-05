using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

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
        private TViewModel _viewModel;                                                                          // viewmodel本體
        private readonly List<IDisposable> _disposables                         = new();                        // 解除訂閱集中管理
        private readonly Dictionary<string, ObservableBinding> _vmObservableMap = new(StringComparer.Ordinal);  // 新增：把 VM 內的 ObservableProperty 索引起來（baseName → 綁定資訊）
        private readonly SpriteCache _spriteCache                               = new();                        // 給圖片綁定用的 Sprite 快取
        private const int TimeToWaitViewModel                                   = 10;        
        
        private void Awake()
        {
            VMLocator.GetOrWaitAsync<TViewModel>(TimeToWaitViewModel, (vm) => 
            {
                _viewModel = vm;

                // 先建立 VM 的 Observable 索引（UI→VM 要用）
                ViewBindingHelper.IndexVmObservables(_viewModel, _vmObservableMap);

                // 再自動註冊 UI 控制的事件（UI→VM）（InputField / Toggle / Slider）
                ViewBindingHelper.AutoRegisterComponents(this, _vmObservableMap);

                // 最後綁訂閱（VM→UI） （文字、Toggle、Slider、Image、RawImage ...）
                ViewBindingHelper.AutoBindObservables(this, _viewModel, _disposables, _spriteCache);

                // ★ 新增：VM→UI（Visible）
                ViewBindingHelper.AutoBindVisibility(this, _vmObservableMap, _disposables);   

                // 衍生類別為內部特定元件時 給予 ViewModel資訊
                if(this is IViewModelGetter<TViewModel>)
                {
                    IViewModelGetter<TViewModel> view = this as IViewModelGetter<TViewModel>;

                    view.OnViewModelReady(_viewModel);
                }
            });
        }


        private void OnDestroy()
        {
            foreach (var d in _disposables)
            {
                d?.Dispose();
            }

            _disposables.Clear();

            // 清除 sprite 快取
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
        protected virtual void OnRefreshLanguage(int currLang)
        {

        }

    }
}
