﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

using XPlan.Utility;

namespace XPlan.UI
{	
	[Serializable]
	struct UIInfo
	{
		[SerializeField]
		public int uiType;

		[SerializeField]
		public GameObject uiGO;

		public UIInfo(int type, GameObject ui)
		{
			uiType	= type;
			uiGO	= ui;
		}
	}

	class UIVisibleInfo
	{
		public GameObject uiIns;
		public int referCount;
		public string uiName;

		public UIVisibleInfo(GameObject u, string s, int r)
		{
			uiIns			= u;
			uiName			= s;
			referCount		= r;
		}
	}

	public class UIController : CreateSingleton<UIController>
    {
		[SerializeField]
		public GameObject uiCanvasGO;

		[SerializeField]
		public GameObject uiBackgroundCanvasGO;

		List<UIVisibleInfo> currVisibleList		= new List<UIVisibleInfo>();
		List<UIVisibleInfo> persistentUIList	= new List<UIVisibleInfo>();
		List<UILoader> loaderStack				= new List<UILoader>();

		protected override void InitSingleton()
		{
	
		}

		/**************************************
		 * 載入流程
		 * ************************************/
		public void LoadingUI(UILoader loader)
		{
			/**************************************
			 * 初始化
			 * ***********************************/
			List<UILoadingInfo> loadingList		= loader.GetLoadingList();
			bool bNeedToDestroyOtherUI			= loader.NeedToDestroyOtherUI();

			Scene currScene						= loader.gameObject.scene;
			int buildIdx						= currScene.buildIndex;

			// 添加新UI的處理
			foreach (UILoadingInfo loadingInfo in loadingList)
			{
				GameObject uiGO = loadingInfo.uiGO;

				if (uiGO == null)
				{
					Debug.LogError("Loading Info is null !");

					continue;
				}

				UIBase ui			= uiGO.GetComponent<UIBase>();
				ui.bSpawnByLoader	= true;

				int idx = currVisibleList.FindIndex((X) =>
				{
					return X.uiName == uiGO.name;
				});

				if (idx == -1)
				{
					GameObject uiIns = GameObject.Instantiate(loadingInfo.uiGO, uiCanvasGO.transform);

					UIStringTable.Instance.InitialUIText(uiIns);

					UIBase[] newUIList = uiIns.GetComponents<UIBase>();

					foreach (UIBase newUI in newUIList)
					{
						newUI.InitialUI(loadingInfo.sortIdx, buildIdx);
					}

					UIVisibleInfo vInfo = new UIVisibleInfo(uiIns, uiGO.name, 1);
					if (loadingInfo.bIsPersistentUI)
					{
						persistentUIList.Add(vInfo);
					}
					else
					{
						currVisibleList.Add(vInfo);
					}
				}
				else
				{
					UIVisibleInfo vInfo = currVisibleList[idx];
					++vInfo.referCount;

					UIBase[] newUIList = vInfo.uiIns.GetComponents<UIBase>();

					foreach (UIBase newUI in newUIList)
					{
						newUI.SortIdx = loadingInfo.sortIdx;
					}
				}
			}

			if(bNeedToDestroyOtherUI)
			{
				for (int i = 0; i < currVisibleList.Count; ++i)
				{
					UIVisibleInfo visibleInfo = currVisibleList[i];

					int idx = loadingList.FindIndex((X) =>
					{
						return X.uiGO.name == visibleInfo.uiName;
					});

					if (idx == -1)
					{
						--visibleInfo.referCount;
					}
				}
			}

			// 移除不需要顯示的UI
			for (int i = currVisibleList.Count - 1; i >= 0; --i)
			{
				UIVisibleInfo visibleInfo = currVisibleList[i];

				if (visibleInfo.referCount <= 0)
				{
					GameObject.DestroyImmediate(visibleInfo.uiIns);
					currVisibleList.RemoveAt(i);
				}
			}

			// 依照sort idx大小由大向小排列
			currVisibleList.Sort((X, Y)=>
			{
				UIBase XUI = X.uiIns.GetComponent<UIBase>();
				UIBase YUI = Y.uiIns.GetComponent<UIBase>();

				return XUI.SortIdx < YUI.SortIdx ?-1:1;
			});

			for(int i = 0; i < currVisibleList.Count; ++i)
			{
				UIVisibleInfo visibleInfo = currVisibleList[i];
				visibleInfo.uiIns.transform.SetSiblingIndex(i);
			}

			loaderStack.Add(loader);
		}

		public void UnloadingUI(UILoader loader)
		{
			List<UILoadingInfo> loadingList = loader.GetLoadingList();

			foreach (UILoadingInfo loadingInfo in loadingList)
			{
				GameObject uiGO = loadingInfo.uiGO;

				if (uiGO == null)
				{
					Debug.LogError("Loading Info is null !");

					continue;
				}

				int idx = currVisibleList.FindIndex((X) =>
				{
					return X.uiName == uiGO.name;
				});

				if (idx != -1)				
				{
					UIVisibleInfo vInfo = currVisibleList[idx];
					--vInfo.referCount;
				}
			}

			for (int i = currVisibleList.Count - 1; i >= 0; --i)
			{
				UIVisibleInfo visibleInfo = currVisibleList[i];

				if (visibleInfo.referCount <= 0)
				{
					GameObject.DestroyImmediate(visibleInfo.uiIns);
					currVisibleList.RemoveAt(i);
				}
			}

			loaderStack.Remove(loader);
		}

		public bool IsWorkingUI(UIBase ui)
		{
			if(loaderStack.Count == 0 && persistentUIList.Count == 0)
			{
				return false;
			}

			// 判斷只有在stack頂層的UI需要做驅動，其他的都視為休息中

			foreach(UIVisibleInfo uiInfo in persistentUIList)
			{
				List<UIBase> uiList = uiInfo.uiIns.GetComponents<UIBase>().ToList();

				if (uiList.Contains(ui))
				{
					return true;
				}
			}
			
			UILoader lastUILoader = loaderStack[loaderStack.Count - 1];

			foreach (UILoadingInfo loadingInfo in lastUILoader.GetLoadingList())
			{
				UIBase[] uiList = loadingInfo.uiGO.GetComponents<UIBase>();

				bool bIsExist = Array.Exists(uiList, (X) => 
				{
					return X.GetType() == ui.GetType();
				});

				if(bIsExist)
				{
					return true;
				}
			}

			return false;
		}

		/**************************************
		 * UI顯示與隱藏
		 * ************************************/
		public void ShowAllUI(bool bEnable)
		{
			uiCanvasGO.SetActive(bEnable);
		}

		public void ShowAllBGUI(bool bEnable)
		{
			uiBackgroundCanvasGO.SetActive(bEnable);
		}
	}
}

