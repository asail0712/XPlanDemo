using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XPlan.Recycle
{
    public class PoolableComponent : MonoBehaviour, IPoolable
    {
        private bool bQuitApp = false;
        private bool bDestroy = false;

        // 可選：給 Pool 指定回收用的 root（避免還掛在 Layout 底下）
        private Transform _recycleRoot;

        public void SetRecycleRoot(Transform recycleRoot)
        {
            _recycleRoot = recycleRoot;
        }

        public void InitialPoolable()
        {
        }

        public void ReleasePoolable()
        {
            if(bQuitApp || bDestroy)
			{                
                return;
			}

            GameObject.DestroyImmediate(gameObject);
        }

        protected void OnDestroy()
		{
            bDestroy = true;
        }

        void OnApplicationQuit()
		{
            bQuitApp = true;
        }

        public virtual void OnSpawn()
        {
            if (bDestroy || this == null) return;

            gameObject.SetActive(true);
        }

        public virtual void OnRecycle()
        {
            if (bDestroy || this == null) return;

            // 移到 poolRoot（避免 LayoutGroup 算到）
            if (_recycleRoot != null)
                transform.SetParent(_recycleRoot, worldPositionStays: false);

            if (gameObject == null)
			{
                return;
			}

            gameObject.SetActive(false);
        }
    }
}
