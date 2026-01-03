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

using XPlan;
using XPlan.Interface;

namespace XPlan.DebugMode
{ 
	public class DebugHandler : LogicComponent, ITickable
	{
		private GameObject debugConsole = null;

		private const string ShowDebugPanel = "ShowDebugPanel";
		private const string HideDebugPanel = "HideDebugPanel";

		public DebugHandler(GameObject console)
		{
			if(console == null)
			{
				return;
			}

			debugConsole = console;
			debugConsole.gameObject.SetActive(false);

			AddUIListener(HideDebugPanel, () =>
			{
				debugConsole.gameObject.SetActive(false);
			});
		}

		public void Tick(float deltaTime)
		{
			if (debugConsole == null)
			{
				return;
			}

			if (Application.isMobilePlatform)
			{
				if (Input.touchCount >= 5)
				{
					debugConsole.SetActive(true);

					DirectCallUI(ShowDebugPanel);
				}
			}
			else
			{
				if (Input.GetKey(KeyCode.BackQuote))
				{
					debugConsole.SetActive(true);

					DirectCallUI(ShowDebugPanel);
				}
			}

			OnUpdate(deltaTime);
		}

		protected virtual void OnUpdate(float deltaTime)
		{

		}
	}
}
