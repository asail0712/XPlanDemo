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
using System.Collections;
using UnityEngine;

namespace XPlan.UI.Fade
{
    public class AlphaFade : FadeBase
    {
        [SerializeField] private float minAlpha = 0.4f;
        [SerializeField] private float maxAlpha = 1f;
        [SerializeField] private float fadeTime = 0.1f;

        private Coroutine fadeCoroutine;

        protected override void FadeIn(Action finishAction)
        {
            if(fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
                fadeCoroutine = null;
            }

            bool bAddNewComponent   = false;
            CanvasGroup cg          = null;

            if(!gameObject.TryGetComponent<CanvasGroup>(out cg))
            {
                cg                  = gameObject.AddComponent<CanvasGroup>();
                bAddNewComponent    = true;
            }

            fadeCoroutine = StartCoroutine(FadeAlpha(cg, minAlpha, maxAlpha, fadeTime, () => 
            {
                if(bAddNewComponent)
                {
                    GameObject.DestroyImmediate(cg);
                }
                
                finishAction?.Invoke();
            }));
        }

        protected override void FadeOut(Action finishAction)
        {
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
                fadeCoroutine = null;
            }

            bool bAddNewComponent   = false;
            CanvasGroup cg          = null;

            if (!gameObject.TryGetComponent<CanvasGroup>(out cg))
            {
                cg                  = gameObject.AddComponent<CanvasGroup>();
                bAddNewComponent    = true;
            }

            StartCoroutine(FadeAlpha(cg, maxAlpha, minAlpha, fadeTime, () =>
            {
                if (bAddNewComponent)
                {
                    GameObject.DestroyImmediate(cg);
                }

                finishAction?.Invoke();
            }));
        }

        private IEnumerator FadeAlpha(CanvasGroup cg, float startAlpha, float targetAlpha, float fadeTime, Action finishAction)
        {
            // 避免第一禎 fps暴衝
            yield return null;

            float currTime  = 0f;
            cg.alpha        = startAlpha;

            while (currTime < fadeTime)
            {
                yield return null;

                cg.alpha = startAlpha + (currTime / fadeTime) * (targetAlpha - startAlpha);
                currTime += Time.deltaTime;
            }

            cg.alpha = targetAlpha;

            finishAction?.Invoke();
        }
    }
}
