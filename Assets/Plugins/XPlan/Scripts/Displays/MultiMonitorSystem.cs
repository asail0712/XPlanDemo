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
using System.Collections.Generic;
using UnityEngine;

namespace XPlan.Displays
{
	[Serializable]
	public class CameraOrderData
	{
		public List<Camera> cameraList;
	}

	[Serializable]
	public class CanvasOrderData
	{
		public List<Canvas> canvasList;
	}

	public class MultiMonitorSystem : SystemBase
    {
		[SerializeField] private string displayOrderFilePath;
		[SerializeField] private List<CameraOrderData> cameraList;
		[SerializeField] private bool bAdjustCanvas = false;

		protected override void OnInitialGameObject()
		{

		}

		protected override void OnInitialLogic()
		{
			RegisterLogic(new DisplayOrderSort(displayOrderFilePath, cameraList, bAdjustCanvas));
		}
	}
}
