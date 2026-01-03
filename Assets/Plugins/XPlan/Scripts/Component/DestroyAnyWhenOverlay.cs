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
	public class DestroyAnyWhenOverlay : MonoBehaviour
	{
        [SerializeField]
        public bool bTriggerCallback        = false;

        [SerializeField]
        public float delayToTrigger         = 0f;

        private Coroutine destroyCoroutine  = null;
        public Action<GameObject> onDestroy;

		private void OnTriggerEnter(Collider other)
		{
            Debug.Log("Trigger Destroy");

            destroyCoroutine = StartCoroutine(DelayedDestroyCoroutine(other.gameObject));
        }

        private void OnDestroy()
        {
            if (destroyCoroutine != null)
            {
                StopCoroutine(destroyCoroutine);
            }
        }

        IEnumerator DelayedDestroyCoroutine(GameObject go)
        {
            // 等待兩秒
            yield return new WaitForSeconds(delayToTrigger);

            if(bTriggerCallback)
			{
                onDestroy?.Invoke(go);
            }
            else
			{
                // 刪除物體
                Destroy(go);
            }
        }
    }
}