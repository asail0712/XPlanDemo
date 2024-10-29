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
