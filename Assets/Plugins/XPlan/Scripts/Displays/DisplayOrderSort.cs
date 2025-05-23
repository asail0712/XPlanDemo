using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

using XPlan;
using XPlan.UI;
using XPlan.Utility;

namespace XPlan.Displays
{
    // 參考文件
    // https://docs.unity3d.com/cn/current/Manual/MultiDisplay.html
    // https://blog.csdn.net/weixin_33912246/article/details/93446238?utm_medium=distribute.pc_relevant.none-task-blog-2~default~baidujs_baidulandingword~default-4-93446238-blog-117528525.235^v43^pc_blog_bottom_relevance_base9&spm=1001.2101.3001.4242.3&utm_relevant_index=7

    public class OrderGroup
    {
		public int displayIdx;
		public List<Camera> cameraList;
		public List<Canvas> canvasList;
	}

    public class DisplayOrderSort : LogicComponent
	{
		private List<OrderGroup> orderGroupList;
		private Canvas[] allCanvases;

		// Start is called before the first frame update
		public DisplayOrderSort(string orderFilePath, List<CameraOrderData> cameraList, bool bAdjustCanvas = false)
        {
			orderGroupList	= new List<OrderGroup>();
			allCanvases		= GameObject.FindObjectsByType<Canvas>(FindObjectsSortMode.None);

			StartCoroutine(AllCamera(orderFilePath, cameraList, bAdjustCanvas));
		}

		private IEnumerator AllCamera(string orderFilePath, List<CameraOrderData> cameraList, bool bAdjustCanvas)
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

			// 將Canvas依照Display或是Camera做分群
			foreach (CameraOrderData orderData in cameraList)
            {
				OrderGroup orderGroup = new OrderGroup();

				if(orderData.cameraList.Count == 0 || orderData.cameraList[0] == null)
                {
					continue;
                }

				orderGroup.displayIdx = orderData.cameraList[0].targetDisplay;
				orderGroup.cameraList = orderData.cameraList;
				orderGroup.canvasList = Array.FindAll<Canvas>(allCanvases, (E04) => 
				{
					CanvasInfo info = E04.GetComponent<CanvasInfo>();

					//期望使用CanvasInfo的資訊來設定 targetDisplay 而不是Canvas本身的設定，因為Canvas的設定可能在上一個Scene被修改過了
					if (info == null)
                    {
						return E04.targetDisplay == orderGroup.displayIdx;
					}
					else
                    {
						return info.defaultDisplayIdx == orderGroup.displayIdx;
					}
					
				}).ToList();

				orderGroupList.Add(orderGroup);
			}

			// 重新設定Display
			OrderAll(orderArr, orderGroupList);

			// 啟動所有使用到的Display
			for (int i = 0; i < Display.displays.Length; ++i)
			{
				Display.displays[i].Activate();
			}
		}

		private void OrderAll(string[] orderArr, List<OrderGroup> orderGroupList)
		{
			// 避免兩者數量不一致
			int totalNum = Mathf.Min(orderArr.Length, orderGroupList.Count);

			// orderArr為Display的順序
			for (int i = 0; i < totalNum; ++i)
			{
				if ("" == orderArr[i])
				{
					continue;
				}

				if (int.TryParse(orderArr[i], out int displayIdx))
				{
					int idx = orderGroupList.FindIndex((E04) => 
					{
						return E04.displayIdx == i;
					});

					if(!orderGroupList.IsValidIndex<OrderGroup>(idx))
                    {
						continue;
                    }

					OrderGroup orderGroup = orderGroupList[idx];

					foreach(Camera camera in orderGroup.cameraList)
                    {
						camera.targetDisplay = displayIdx - 1;
					}

					foreach (Canvas canvas in orderGroup.canvasList)
					{
						canvas.targetDisplay = displayIdx - 1;
					}
				}
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
