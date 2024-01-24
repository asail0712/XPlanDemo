using System;
using UnityEngine.UI;

using XPlan.Interface;

namespace XPlan.UI
{
    public class SingleClickFilter : IButton
    {
        public event Action OnClick;

        private readonly IStateValue<bool> isClicked;

        /************************************
         * toggle流程 
         ***********************************/
        public SingleClickFilter(Toggle toggle, IStateValue<bool> isClicked)
        {
            this.isClicked  = isClicked;
            // 受到isClicked.State影響，不管掛了幾個callback都只會執行第一個，因此加掛時會先清楚之前的callback比較合理
            toggle.onValueChanged.RemoveAllListeners();
            toggle.onValueChanged.AddListener((bIsClick)=> 
            {
                if (!bIsClick)
                {
                    return;
                }

                if (isClicked.State == true)
                {
                    return;
                }

                isClicked.State = true;

                OnClick?.Invoke();
            });
        }

        /************************************
         * button流程 
         ***********************************/
        public SingleClickFilter(Button button, IStateValue<bool> isClicked)
        {
            this.isClicked = isClicked;

            // 受到isClicked.State影響，不管掛了幾個callback都只會執行第一個，因此加掛時會先清楚之前的callback比較合理
            button.onClick.RemoveAllListeners();

            button.onClick.AddListener(OnClickButtonHandler);
        }

        private void OnClickButtonHandler()
        {
            if (isClicked.State == true)
            {
                return;
            }

            isClicked.State = true;

            OnClick?.Invoke();
        }
    }
}