using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XPlan.UI;

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

                // UICintroller必須比UILoader早啟動
                // 將有UIController的物件提前到第一個
                Queue<DebugCheck> debugCheckQueue   = new Queue<DebugCheck>(debugCheckArr);
                int currCount                       = 0;
                UIController dummy                  = null;

                while (currCount++ < debugCheckQueue.Count)
				{
                    DebugCheck debugCheck = debugCheckQueue.Peek();

                    if (debugCheck.gameObject.TryGetComponent<UIController>(out dummy))
					{
                        break;
					}

                    debugCheckQueue.Enqueue(debugCheckQueue.Dequeue());
                }
                
                // 更換完順序開始啟動
                foreach(DebugCheck debugCheck in debugCheckQueue)
				{
                    debugCheck.gameObject.SetActive(true);
                }
            }

            // 設定完就將該功能關閉
            gameObject.SetActive(false);
        }
    }
}
