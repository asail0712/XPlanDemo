using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XPlan.Utility;

namespace XPlan.Utility
{
    public static class UIExtensions
    {
        static public void FadeInOutBtnAlpha(this CanvasGroup canvasGroup, float targetAlpha, float duration, Action finishAction = null)
		{
            MonoBehaviourHelper.StartCoroutine(FadeInOutBtnAlpha_Internal(canvasGroup, targetAlpha, duration, finishAction));
		}

        static private IEnumerator FadeInOutBtnAlpha_Internal(this CanvasGroup canvasGroup, float targetAlpha, float duration, Action finishAction = null)
		{
            float elapsedTime   = 0f;
            float startAlpha    = canvasGroup.alpha;

            while (elapsedTime < duration)
            {
                yield return null;
                elapsedTime         += Time.deltaTime;
                float newAlpha      = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / duration);
                canvasGroup.alpha   = newAlpha;
            }

            canvasGroup.alpha = targetAlpha;

            finishAction?.Invoke();
        }
    }
}