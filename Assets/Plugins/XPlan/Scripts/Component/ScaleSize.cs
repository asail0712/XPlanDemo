using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace XPlan.Component
{
    public class ScaleSize : MonoBehaviour
    {
        [SerializeField] public Vector3 targetSize;
        [SerializeField] public float scaleTime;

        private Vector3 startSize;
        private float currTime;

        public void StartToScale(Action finishAction = null)
		{
            startSize   = transform.localScale;
            currTime    = 0f;

            StartCoroutine(StartToScale_Internal(finishAction));
		}

        private IEnumerator StartToScale_Internal(Action finishAction)
		{
            while(currTime < scaleTime)
			{
                yield return null;
                transform.localScale = Vector3.Lerp(startSize, targetSize, currTime);

                currTime += Time.deltaTime;
            }

            transform.localScale = targetSize;

            finishAction?.Invoke();
        }
    }
}
