using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XPlan.DebugMode
{
    public class DebugCheck : MonoBehaviour
    {
        // Start is called before the first frame update
        void Awake()
        {
            if (DebugManager.IsInitial())
            {
                // Debug Manager有Initial的話，表示不是單一Scene獨立測試
                // 就把該物件視為Debug物件而關閉
                gameObject.SetActive(false);
            }
        }
    }
}
