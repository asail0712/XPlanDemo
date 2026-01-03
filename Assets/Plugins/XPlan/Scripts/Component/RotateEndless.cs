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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XPlan.Components
{
    public class RotateEndless : MonoBehaviour
    {
		[SerializeField]
		private Vector3 axis = Vector3.up;

		[SerializeField]
		private float rotationSpeed = 50.0f;

		void Update()
		{
			transform.Rotate(axis, Time.deltaTime * rotationSpeed);
		}
	}
}
