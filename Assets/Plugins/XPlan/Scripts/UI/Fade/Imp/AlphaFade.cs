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

        protected override void FadeIn(Action finishAction)
        {
            CanvasGroup cg  = gameObject.AddComponent<CanvasGroup>();

            StartCoroutine(FadeAlpha(cg, minAlpha, maxAlpha, fadeTime, () => 
            {
                GameObject.DestroyImmediate(cg);

                finishAction?.Invoke();
            }));
        }

        protected override void FadeOut(Action finishAction)
        {
            CanvasGroup cg = gameObject.AddComponent<CanvasGroup>();

            StartCoroutine(FadeAlpha(cg, maxAlpha, minAlpha, fadeTime, () =>
            {
                GameObject.DestroyImmediate(cg);

                finishAction?.Invoke();
            }));
        }

        private IEnumerator FadeAlpha(CanvasGroup cg, float startAlpha, float targetAlpha, float fadeTime, Action finishAction)
        {
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
