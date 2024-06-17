using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XPlan.Component
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