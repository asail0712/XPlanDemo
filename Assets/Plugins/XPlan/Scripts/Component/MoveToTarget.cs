using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace XPlan.Component
{
    public class MoveToTarget : MonoBehaviour
    {
        [SerializeField] public Vector3 targetPos;
        [SerializeField] public float moveTime;

        private Vector3 startPosition;
        private float currTime;

        public void StartToMove(Action finishAction = null)
		{
            startPosition   = transform.position;
            currTime        = 0f;

            StartCoroutine(StartToMove_Internal(finishAction));
		}

        private IEnumerator StartToMove_Internal(Action finishAction)
		{
            while(currTime < moveTime)
			{
                yield return null;
                transform.position = Vector3.Lerp(startPosition, targetPos, currTime);

                currTime += Time.deltaTime;
            }

            transform.position = targetPos;

            finishAction?.Invoke();
        }
    }
}
