using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
		public int rootIdx;
		public int referCount;
		public string uiName;

		public UIVisibleInfo(GameObject u, string s, int r, int i)
		{
			uiIns			= u;
			uiName			= s;
			referCount		= r;
			rootIdx			= i;
		}
	}

	public class UIController : CreateSingleton<UIController>
    {
		[SerializeField]
		public List<GameObject> uiRootList;

		[SerializeField]
		public TextAsset[] csvAssetList;

		private List<UIVisibleInfo> currVisibleList		= new List<UIVisibleInfo>();
		private List<UIVisibleInfo> persistentUIList	= new List<UIVisibleInfo>();
		private List<UILoader> loaderStack				= new List<UILoader>();
		private StringTable stringTable					= new StringTable();

		protected override void InitSingleton()
		{
			stringTable.InitialStringTable(csvAssetList);
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

			// 添加新UI的處理
			foreach (UILoadingInfo loadingInfo in loadingList)
			{
				/********************************
				 * 確認 perfab
				 * *****************************/
				GameObject uiPerfab = loadingInfo.uiPerfab;

				if (uiPerfab == null)
				{
					LogSystem.Record("Loading Info is null !", LogType.Error);

					continue;
				}

				/********************************
				 * 判斷該UI是否已經在畫面上
				 * *****************************/
				GameObject uiIns	= null;
				int idx				= currVisibleList.FindIndex((X) =>
				{
					return X.uiName == uiPerfab.name && X.rootIdx == loadingInfo.rootIdx;
				});

				if (idx == -1)
				{
					// 確認加載 UI Root
					if(!uiRootList.IsValidIndex<GameObject>(loadingInfo.rootIdx)
						|| uiRootList[loadingInfo.rootIdx] == null)
					{
						LogSystem.Record($"{loadingInfo.rootIdx} 是無效的rootIdx", LogType.Warning);
						continue;
					}

					// 生成UI
					uiIns = GameObject.Instantiate(loadingInfo.uiPerfab, uiRootList[loadingInfo.rootIdx].transform);

					// 強制enable 去觸發 Awake，來註冊Command
					uiIns.SetActive(true);
					uiIns.transform.localScale = Vector3.zero;

					// 加上文字
					stringTable.InitialUIText(uiIns);

					// 初始化所有的 ui base
					UIBase[] newUIList = uiIns.GetComponents<UIBase>();

					if (newUIList == null || newUIList.Length == 9)
					{
						LogSystem.Record("uiBase is null !", LogType.Error);

						continue;
					}

					foreach (UIBase newUI in newUIList)
					{
						newUI.InitialUI(loadingInfo.sortIdx);
					}

					// 確認是否為常駐UI
					UIVisibleInfo vInfo = new UIVisibleInfo(uiIns, uiPerfab.name, 1, loadingInfo.rootIdx);
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
					uiIns				= vInfo.uiIns;
					UIBase[] newUIList	= uiIns.GetComponents<UIBase>();

					foreach (UIBase newUI in newUIList)
					{
						newUI.SortIdx = loadingInfo.sortIdx;
					}
				}

				StartCoroutine(UIVisibleSetting(uiIns, loadingInfo.bVisible));
			}

			/********************************
			 * 判斷是否有UI需要移除
			 * *****************************/
			if (bNeedToDestroyOtherUI)
			{
				for (int i = 0; i < currVisibleList.Count; ++i)
				{
					UIVisibleInfo visibleInfo = currVisibleList[i];

					int idx = loadingList.FindIndex((X) =>
					{
						return X.uiPerfab.name == visibleInfo.uiName;
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

			/********************************
			 * 將剩下的UI依照順序排列
			 * *****************************/
			List<UIVisibleInfo> sortUIList = new List<UIVisibleInfo>();
			sortUIList.AddRange(currVisibleList);
			sortUIList.AddRange(persistentUIList);

			// 依照sort idx大小由大向小排列
			sortUIList.Sort((X, Y)=>
			{
				UIBase XUI = X.uiIns.GetComponent<UIBase>();
				UIBase YUI = Y.uiIns.GetComponent<UIBase>();

				return XUI.SortIdx < YUI.SortIdx ?-1:1;
			});

			for (int i = 0; i < sortUIList.Count; ++i)
			{
				UIVisibleInfo visibleInfo = sortUIList[i];
				visibleInfo.uiIns.transform.SetSiblingIndex(i);
			}

			loaderStack.Add(loader);			
		}

		private IEnumerator UIVisibleSetting(GameObject uiIns, bool bVisible)
		{
			yield return new WaitForEndOfFrame();
			
			/********************************
			* 設定UI Visible
			* *****************************/			
			
			if (uiIns != null)
			{
				uiIns.SetActive(bVisible);
				uiIns.transform.localScale = Vector3.one;
			}
		}

		public void UnloadingUI(UILoader loader)
		{
			List<UILoadingInfo> loadingList = loader.GetLoadingList();

			foreach (UILoadingInfo loadingInfo in loadingList)
			{
				GameObject uiGO = loadingInfo.uiPerfab;

				if (uiGO == null)
				{
					Debug.LogError("Loading Info is null !");

					continue;
				}

				int idx = currVisibleList.FindIndex((X) =>
				{
					return X.uiName == uiGO.name && X.rootIdx == loadingInfo.rootIdx;
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
				UIBase[] uiList = loadingInfo.uiPerfab.GetComponents<UIBase>();

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
		public void SetUIVisible<T>(bool bEnable) where T : UIBase
		{
			List<UIVisibleInfo> allUIList = new List<UIVisibleInfo>();
			allUIList.AddRange(currVisibleList);
			allUIList.AddRange(persistentUIList);

			bool bIsFinded = false;

			foreach (UIVisibleInfo uiInfo in allUIList)
			{
				List<UIBase> uiList = uiInfo.uiIns.GetComponents<UIBase>().ToList();

				foreach (UIBase ui in uiList)
				{ 
					if(ui is T)
					{
						bIsFinded = true;
						break;
					}
				}

				if(bIsFinded)
				{
					uiInfo.uiIns.SetActive(bEnable);
					break;
				}
			}
		}

		public void SetRootVisible(bool bEnable, int rootIdx = -1)
		{
			if (!uiRootList.IsValidIndex<GameObject>(rootIdx))
			{
				LogSystem.Record($"{rootIdx} 為無效的root idx", LogType.Warning);
				return;
			}

			uiRootList[rootIdx].SetActive(bEnable);
		}

		public void SetAllUIVisible(bool bEnable)
		{
			// 若是index不存在
			uiRootList.ForEach((X)=> 
			{
				X.SetActive(bEnable);
			});
		}

		/**************************************
		 * String Table
		 * ************************************/
		public string GetStr(string keyStr, bool bShowWarning = false)
		{
			return stringTable.GetStr(keyStr, bShowWarning);
		}
	}
}

