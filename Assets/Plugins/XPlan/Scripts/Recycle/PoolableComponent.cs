using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XPlan.Recycle
{
    public class PoolableComponent : MonoBehaviour, IPoolable
    {
        private bool bQuitApp = false;
        private bool bBeDestroy = false;

        public void InitialPoolable()
        {
        }

        public void ReleasePoolable()
        {
            if(bQuitApp || bBeDestroy)
			{                
                return;
			}

            GameObject.DestroyImmediate(gameObject);
        }

        void OnDestroy()
		{
            bBeDestroy = true;
        }

        void OnApplicationQuit()
		{
            bQuitApp = true;
        }

        public void OnSpawn()
        {
            gameObject.SetActive(true);
        }

        public void OnRecycle()
        {
            gameObject.SetActive(false);
        }
    }
}
