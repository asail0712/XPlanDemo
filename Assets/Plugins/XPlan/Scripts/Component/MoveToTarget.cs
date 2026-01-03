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
using System.Collections.Generic;

using UnityEngine;

namespace XPlan.Components
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
