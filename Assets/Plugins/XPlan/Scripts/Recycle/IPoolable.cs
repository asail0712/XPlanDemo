using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XPlan.Recycle
{ 
    public interface IPoolable
    {
        void InitialPoolable();
        void ReleasePoolable();

        void OnSpawn();
        void OnRecycle();
    }
}
