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
		[SerializeField] private List<CanvasOrderData> canvasArr;

		protected override void OnInitialGameObject()
		{

		}

		protected override void OnInitialLogic()
		{
			RegisterLogic(new DisplayOrderSort(displayOrderFilePath, cameraList, canvasArr));
		}
	}
}
