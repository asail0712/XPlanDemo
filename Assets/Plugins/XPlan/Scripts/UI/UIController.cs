using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

using XPlan.UI.Components;
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
        public List<IUIView> uiList;

		public UIVisibleInfo(GameObject u, string s, int r, int i)
		{
			uiIns			= u;
			uiName			= s;
			referCount		= r;
			rootIdx			= i;
            uiList          = uiIns.GetInterfaces<IUIView>();
        }
	}

	public class UIController : CreateSingleton<UIController>
    {
		[SerializeField]
		public List<GameObject> uiRootList;

		[SerializeField]
		public TextAsset[] csvAssetList;

		private int currQuality;
        public int CurrQuality
		{
			get
			{
				return currQuality;
			}
			set
			{
				currQuality = value;
				QualityChange();
            }
		}

        public int CurrLanguage
		{
			get
			{
				return StringTable.Instance.CurrLanguage;
			}
			set
			{
                StringTable.Instance.CurrLanguage = value;
			}
		}

		private List<UIVisibleInfo> currVisibleList		= new List<UIVisibleInfo>();
		private List<UIVisibleInfo> persistentUIList	= new List<UIVisibleInfo>();
		private List<UILoader> loaderStack				= new List<UILoader>();

		protected override void InitSingleton()
		{
            // 設定多語言字串表
            StringTable.Instance.InitialStringTable(csvAssetList);

			// 初始化靜態UI
			InitialStaticUI();
		}

		/**************************************
		 * 靜態UI處理
		 * ************************************/
		private void InitialStaticUI()
		{
            if (uiRootList == null || uiRootList.Count == 0) return;

            // 用 HashSet 去重，比 List.AddUnique 更快更乾淨
            HashSet<GameObject> uiGOSet = new HashSet<GameObject>();

            // 蒐集所有 IUIView，初始化，並同時收集對應的 GameObject
            foreach (var uiRoot in uiRootList)
            {
                if (!uiRoot) continue;

                // 這裡用你前面做好的介面搜尋工具
                var views = uiRoot.GetInterfacesInChildren<IUIView>();
                foreach (var view in views)
                {
                    // 以 Component 來取得 gameObject；避免假設 IUIView 介面本身有 gameObject 屬性
                    var comp = view as Component;
                    if (comp)
                    {
                        uiGOSet.Add(comp.gameObject);
                    }

                    view.SortIdx = -1;
                }
            }
			
			foreach (GameObject ui in uiGOSet)
			{
                // 設定字表
                StringTable.Instance.InitialUIText(ui);

                // 處理Quality
                RefreshQuality(ui, currQuality);

                // 加入 currVisibleList
                currVisibleList.Add(new UIVisibleInfo(ui, ui.name, 1, 0));
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
                    StringTable.Instance.InitialUIText(uiIns);

					// 處理Quality
					RefreshQuality(uiIns, currQuality);

                    // 初始化所有的 ui base
                    List<IUIView> newUIList = uiIns.GetInterfaces<IUIView>();

					if (newUIList == null)
					{
						LogSystem.Record("View is null !", LogType.Error);

						continue;
					}

					foreach (IUIView newUI in newUIList)
					{
						newUI.SortIdx = loadingInfo.sortIdx;
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
					UIVisibleInfo vInfo		= currVisibleList[idx];
					++vInfo.referCount;
					uiIns					= vInfo.uiIns;
					List<IUIView> newUIList	= uiIns.GetInterfaces<IUIView>();

                    foreach (IUIView newUI in newUIList)
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
                IUIView XUI = X.uiIns.GetInterface<IUIView>();
                IUIView YUI = Y.uiIns.GetInterface<IUIView>();

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

		/**************************************
		 * UI顯示與隱藏
		 * ************************************/
		public void SetUIVisible<T>(bool bEnable) where T : IUIView
		{
			List<UIVisibleInfo> allUIList = new List<UIVisibleInfo>();
			allUIList.AddRange(currVisibleList);
			allUIList.AddRange(persistentUIList);

			bool bIsFinded = false;

			foreach (UIVisibleInfo uiInfo in allUIList)
			{
				List<IUIView> uiList = uiInfo.uiIns.GetInterfaces<IUIView>();

				foreach (IUIView ui in uiList)
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

        /**************************************
		 * Quality
		 * ************************************/
        private void QualityChange()
		{
            List<GameObject> allVisibleUIs = UIController.Instance.GetAllVisibleUI();

			allVisibleUIs.ForEach(e04 => RefreshQuality(e04, currQuality));
        }

		private void RefreshQuality(GameObject uiGO, int quality)
		{
            QualitySpriteProvider qualityProvider = uiGO.AddOrFindComponent<QualitySpriteProvider>();

            qualityProvider.RefreshImage(quality);
        }
    }
}

