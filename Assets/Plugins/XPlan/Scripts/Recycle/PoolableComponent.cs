using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XPlan.Recycle
{
    public class PoolableComponent : MonoBehaviour, IPoolable
    {
        public void InitialPoolable()
        {
        }

        public void ReleasePoolable()
        {
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
