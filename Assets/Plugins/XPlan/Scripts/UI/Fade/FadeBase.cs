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
