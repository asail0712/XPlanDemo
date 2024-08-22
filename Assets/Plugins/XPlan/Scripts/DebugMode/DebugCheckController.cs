using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XPlan.DebugMode
{ 
    public class DebugCheckController : MonoBehaviour
    {
        // Start is called before the first frame update
        void Awake()
        {
            // Debug Manager有Initial的話，表示不是單一Scene獨立測試
            // 就把該物件視為Debug物件而關閉
            if (!DebugManager.IsInitial())
            {
                DebugCheck[] debugCheckArr = FindObjectsOfType<DebugCheck>(true);
                
                foreach(DebugCheck debugCheck in debugCheckArr)
				{
                    debugCheck.gameObject.SetActive(true);
                }
            }

            // 設定完就將該功能關閉
            gameObject.SetActive(false);
        }
    }
}
