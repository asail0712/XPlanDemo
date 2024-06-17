using System;

using XPlan.Interface;

namespace XPlan.UI
{
    public class ButtonLazyAdapter : Lazy<IButton>, IButton
    {
        public ButtonLazyAdapter(Func<IButton> valueFactory) : base(valueFactory)
        {
        }

        public event Action OnClick
        {
            add
            {
                Value.OnClick += value;
            }
            remove
            {
                Value.OnClick -= value;
            }
        }
    }
}