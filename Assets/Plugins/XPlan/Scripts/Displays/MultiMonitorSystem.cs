using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XPlan.Displays
{
	public class MultiMonitorSystem : SystemBase
    {
		[SerializeField] private string displayOrderFilePath;
		[SerializeField] private List<Camera> cameraList;
		[SerializeField] private List<Canvas> canvasArr;

		protected override void OnInitialGameObject()
		{

		}

		protected override void OnInitialHandler()
		{
			RegisterLogic(new DisplayOrderSort(displayOrderFilePath, cameraList, canvasArr));
		}
	}
}
