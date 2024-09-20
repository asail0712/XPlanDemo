using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

using XPlan;
using XPlan.Extensions;
using XPlan.Utility;

namespace XPlan.Displays
{
	// 參考文件
	// https://docs.unity3d.com/cn/current/Manual/MultiDisplay.html
	// https://blog.csdn.net/weixin_33912246/article/details/93446238?utm_medium=distribute.pc_relevant.none-task-blog-2~default~baidujs_baidulandingword~default-4-93446238-blog-117528525.235^v43^pc_blog_bottom_relevance_base9&spm=1001.2101.3001.4242.3&utm_relevant_index=7

	public class DisplayOrderSort : LogicComponentBase
	{
		// Start is called before the first frame update
		public DisplayOrderSort(string orderFilePath, List<Camera> cameraList, List<Canvas> canvasList = null)
        {
			StartCoroutine(AllCamera(orderFilePath, cameraList, canvasList));
		}

		private IEnumerator AllCamera(string orderFilePath, List<Camera> cameraList, List<Canvas> canvasList)
		{
			string[] orderArr = null;

			// 對Camera對應的Display做修改
			yield return ReadDisplayOrder(orderFilePath, (orderStr) =>
			{
				orderArr = orderStr.Split(",");
			});

			if(orderArr == null)
			{
				LogSystem.Record($"{orderFilePath} 沒有找到可以排序的資料 !!", LogType.Warning);

				yield break;
			}

			CameraOrder(orderArr, cameraList);

			if(canvasList != null)
			{
				CanvasOrder(orderArr, canvasList);
			}
			
			// 啟動所有使用到的Display
			for (int i = 0; i < Display.displays.Length; ++i)
			{
				Display.displays[i].Activate();
			}
		}

		private void CameraOrder(string[] orderArr, List<Camera> cameraList)
		{			
			if (orderArr.Length != cameraList.Count)
			{
				LogSystem.Record($"{cameraList} 數量 與 外部檔案數量 不一致!!", LogType.Warning);
			}

			// 避免兩者數量不一致
			int totalNum = Mathf.Min(orderArr.Length, cameraList.Count);

			// orderArr為Display的順序
			for (int i = 0; i < totalNum; ++i)
			{
				if ("" == orderArr[i])
				{
					continue;
				}

				if (int.TryParse(orderArr[i], out int displayIdx))
				{
					cameraList[i].targetDisplay = displayIdx - 1;
				}
				else
				{
					Debug.LogError($"display order的內容 {orderArr[i]} 無法解譯");
				}
			}
		}

		private void CanvasOrder(string[] orderArr, List<Canvas> canvasList)
		{
			if (orderArr.Length != canvasList.Count)
			{
				LogSystem.Record($"{canvasList} 數量 與 外部檔案數量 不一致!!", LogType.Warning);
			}

			// 避免兩者數量不一致
			int totalNum = Mathf.Min(orderArr.Length, canvasList.Count);

			// orderArr為Display的順序
			for (int i = 0; i < totalNum; ++i)
			{
				if ("" == orderArr[i])
				{
					continue;
				}

				if (int.TryParse(orderArr[i], out int displayIdx))
				{
					canvasList[i].targetDisplay = displayIdx - 1;
				}
				else
				{
					Debug.LogError($"display order的內容 {orderArr[i]} 無法解譯");
				}
			}
		}

		protected override void OnDispose(bool bAppQuit)
		{
			if(bAppQuit)
			{
				return;
			}
		}

		private IEnumerator ReadDisplayOrder(string orderFilePath, Action<string> finishAction)
		{
			string url				= new Uri(orderFilePath).AbsoluteUri;
			UnityWebRequest request = UnityWebRequest.Get(url);
			request.downloadHandler = new DownloadHandlerBuffer();

			yield return request.SendWebRequest();

			if (request.result != UnityWebRequest.Result.Success)
			{
				finishAction?.Invoke("");

				yield break;
			}

			finishAction?.Invoke(request.downloadHandler.text);
		}
	}
}
