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

using XPlan.UI;

namespace XPlan.DebugMode
{ 
    public class DebugCheckController : MonoBehaviour
    {
        // Start is called before the first frame update
        void Awake()
        {
            DebugCheck[] debugCheckArr          = FindObjectsByType<DebugCheck>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            Queue<DebugCheck> debugCheckQueue   = new Queue<DebugCheck>(debugCheckArr);

            // Debug Manager有Initial的話，表示不是單一Scene獨立測試
            // 就把該物件視為Debug物件而關閉
            if (!DebugManager.IsInitial())
            {
                // UICintroller必須比UILoader早啟動
                // 將有UIController的物件提前到第一個
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

                foreach (DebugCheck debugCheck in debugCheckQueue)
				{
                    debugCheck.gameObject.SetActive(true);
                }
            }
			else
			{
                while (debugCheckQueue.Count > 0)
                {
                    DebugCheck debugCheck = debugCheckQueue.Dequeue();

                    GameObject.DestroyImmediate(debugCheck.gameObject);
                }
            }

            // 設定完就將該功能關閉
            gameObject.SetActive(false);
        }
    }
}
