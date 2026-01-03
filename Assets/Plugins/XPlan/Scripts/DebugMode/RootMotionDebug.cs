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

namespace XPlan.DebugMode
{
    public class RootMotionDebug : MonoBehaviour
    {
        [SerializeField]
        private Animator animator;

        void Start()
        {
            if (animator == null)
            {
                Debug.LogError("Animator component is missing.");
            }
        }

        void OnAnimatorMove()
        {
            if (animator)
            {
                Debug.Log("OnAnimatorMove called");
                Debug.Log("deltaPosition: " + animator.deltaPosition);
                Debug.Log("deltaRotation: " + animator.deltaRotation);

                // 应用Root Motion到Transform上
                transform.position += animator.deltaPosition;
                transform.rotation *= animator.deltaRotation;
            }
        }
    }
}