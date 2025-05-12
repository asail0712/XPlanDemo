using System;
using UnityEngine;

namespace XPlan.UI.Fade
{
    public enum FadeType
    {
        In,
        Out,
        InAndOut,
    }

    public class FadeBase : MonoBehaviour
    {
        [SerializeField] protected FadeType type = FadeType.InAndOut;

        public void PleaseStartYourPerformance(bool bEnabled, Action finishAction)
        {
            if (bEnabled)
            {
                if(type == FadeType.In || type == FadeType.InAndOut)
                {
                    FadeIn(finishAction);
                }
                else
                {
                    finishAction?.Invoke();
                }                
            }
            else
            {
                if (type == FadeType.Out || type == FadeType.InAndOut)
                {
                    FadeOut(finishAction);
                }
                else
                {
                    finishAction?.Invoke();
                }
            }
        }

        protected virtual void FadeIn(Action finishAction)
        {
            finishAction?.Invoke();
        }

        protected virtual void FadeOut(Action finishAction)
        {
            finishAction?.Invoke();
        }
    }
}
