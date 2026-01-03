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
