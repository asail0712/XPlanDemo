using System;
using System.Collections;
using UnityEngine;

namespace XPlan.UI.Fade
{
    public class ScaleFade : FadeBase
    {
        [SerializeField] private float minScale = 0.5f;
        [SerializeField] private float maxScale = 1f;
        [SerializeField] private float fadeTime = 0.1f;

        protected override void FadeIn(Action finishAction)
        {
            Transform transform = gameObject.GetComponent<Transform>();

            StartCoroutine(FadeScale(transform, minScale, maxScale, fadeTime, finishAction));
        }

        protected override void FadeOut(Action finishAction)
        {
            Transform transform = gameObject.GetComponent<Transform>();

            StartCoroutine(FadeScale(transform, maxScale, minScale, fadeTime, finishAction));
        }

        private IEnumerator FadeScale(Transform transform, float startScale, float targetScale, float fadeTime, Action finishAction)
        {
            float currTime          = 0f;
            transform.localScale    = new Vector3(startScale, startScale, startScale);

            while (currTime < fadeTime)
            {
                yield return null;

                float currScale         = startScale + (currTime / fadeTime) * (targetScale - startScale);
                transform.localScale    = new Vector3(currScale, currScale, currScale);
                currTime                += Time.deltaTime;
            }

            transform.localScale = new Vector3(targetScale, targetScale, targetScale);

            finishAction?.Invoke();
        }
    }
}
