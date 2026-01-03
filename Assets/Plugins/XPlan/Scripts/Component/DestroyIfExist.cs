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
using UnityEngine;

namespace XPlan.Components
{ 
    public class DestroyIfExist : MonoBehaviour
    {
	    void Awake()
	    {
            GameObject[] objects    = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            int repeatNum           = 0;

            foreach (GameObject obj in objects)
            {
                if (obj.name == gameObject.name)
                {
                    ++repeatNum;
                }
            }

            if(repeatNum > 1)
		    {
                Debug.Log($"{gameObject.name} need to be Destroy !!");
                DestroyImmediate(gameObject);
		    }
        }
    }
}
