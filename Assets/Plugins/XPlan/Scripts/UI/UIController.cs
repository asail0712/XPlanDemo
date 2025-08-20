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
        public List<UIBase> uiList;

		public UIVisibleInfo(GameObject u, string s, int r, int i)
		{
			uiIns			= u;
			uiName			= s;
			referCount		= r;
			rootIdx			= i;
            uiList          = uiIns.GetComponents<UIBase>().ToList();
        }
	}

	public class UIController : CreateSingleton<UIController>
    {
		[SerializeField]
		public List<GameObject> uiRootList;

		[SerializeField]
		public TextAsset[] csvAssetList;

		public int CurrLanguage
		{
			get
			{
				return stringTable.CurrLanguage;
			}
			set
			{
				stringTable.CurrLanguage = value;
			}
		}

		private List<UIVisibleInfo> currVisibleList		= new List<UIVisibleInfo>();
		private List<UIVisibleInfo> persistentUIList	= new List<UIVisibleInfo>();
		private List<UILoader> loaderStack				= new List<UILoader>();
		private StringTable stringTable					= new StringTable();

		protected override void InitSingleton()
		{
			// 設定多語言字串表
			stringTable.InitialStringTable(csvAssetList);

			// 初始化靜態UI
			InitialStaticUI();
		}

		/**************************************
		 * 靜態UI處理
		 * ************************************/
		private void InitialStaticUI()
		{
			List<GameObject> uiGOList	= new List<GameObject>();
			List<UIBase> uiList			= new List<UIBase>();

			foreach (GameObject uiRoot in uiRootList)
			{
				uiList.AddRange(uiRoot.GetComponentsInChildren<UIBase>().ToList());
			}

			// 初始化
			foreach (UIBase ui in uiList)
			{
				uiGOList.AddUnique<GameObject>(ui.gameObject);

				ui.InitialUI(-1);
			}

			// 設定字表
			foreach (GameObject ui in uiGOList)
			{
				stringTable.InitialUIText(ui);
			}
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

					// 加上文字
					stringTable.InitialUIText(uiIns);

					// 初始化所有的 ui base
					UIBase[] newUIList = uiIns.GetComponents<UIBase>();

					if (newUIList == null)
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

				// 設定UI Visible
				if (uiIns != null)
				{
					uiIns.SetActive(loadingInfo.bVisible);
					uiIns.transform.localScale = Vector3.one;
				}

				loader.IsDone = true;
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
            // 先判斷常駐UI
			foreach(UIVisibleInfo uiInfo in persistentUIList)
			{
				List<UIBase> uiList = uiInfo.uiList;

				if (uiList.Contains(ui))
				{
					return true;
				}
			}
			
            // 判斷非常駐UI中最新的UILoader
			UILoader lastUILoader = loaderStack[loaderStack.Count - 1];

			foreach (UILoadingInfo loadingInfo in lastUILoader.GetLoadingList())
			{
				UIBase[] uiList = loadingInfo.uiList;
				bool bIsExist   = Array.Exists(uiList, (X) => 
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
		public string GetStr(string keyStr)
		{
			return stringTable.GetStr(keyStr);
		}

		public string ReplaceStr(string keyStr, params string[] paramList)
		{
			return stringTable.ReplaceStr(keyStr, paramList);
		}

		public List<GameObject> GetAllVisibleUI()
		{
			List<GameObject> result = new List<GameObject>();

			foreach (UIVisibleInfo info in currVisibleList)
			{
				result.Add(info.uiIns);
			}

			foreach (UIVisibleInfo info in persistentUIList)
			{
				result.Add(info.uiIns);
			}

			return result;
		}
	}
}

