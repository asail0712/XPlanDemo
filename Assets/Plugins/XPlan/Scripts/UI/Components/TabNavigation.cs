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
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace XPlan.UI.Components
{
    public class TabNavigation : MonoBehaviour
    {
        [SerializeField] private Selectable[] selectableList; // 依順序指定

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                GameObject current = EventSystem.current.currentSelectedGameObject;

                for (int i = 0; i < selectableList.Length; i++)
                {
                    if (current == selectableList[i].gameObject)
                    {
                        int nextIndex = (i + 1) % selectableList.Length; // 循環
                        selectableList[nextIndex].Select();
                        break;
                    }
                }
            }
        }
    }
}
