using UnityEngine;

namespace XPlan.UI
{
    internal interface IViewModelGetter<TViewModel> where TViewModel : ViewModelBase
    {
        void OnViewModelReady(TViewModel vm);
    }
}