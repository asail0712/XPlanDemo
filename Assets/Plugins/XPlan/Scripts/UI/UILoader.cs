using System;
using System.Collections.Generic;
using UnityEngine;

#if ADDRESSABLES_EXISTS
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
#endif

namespace XPlan.UI
{
	[Serializable]
	public class UILoadingInfo
	{
		[SerializeField] public GameObject uiPerfab;

        [SerializeField] public string addressNameStr;

        [SerializeField] public int rootIdx;

		[SerializeField] public int sortIdx;

		[SerializeField] public bool bIsPersistentUI;

		[SerializeField] public bool bVisible;

        [HideInInspector] public UIBase[] uiList;

		public UILoadingInfo()
		{
			rootIdx			= 0;
			sortIdx			= 5;
			bIsPersistentUI = false;
			bVisible		= true;
        }
	}

    public class UILoader : MonoBehaviour
    {
		[SerializeField] private List<UILoadingInfo> loadingList	= new List<UILoadingInfo>();
        [SerializeField] private bool bDestroyOtherUI				= false;

		private bool bCancalUnload	= false;
		public bool IsDone			= false;

		private void Awake()
		{
			int sumOfLoading	= 0;
            int currLoading		= 0;

#if ADDRESSABLES_EXISTS
            // 加上 addressable 的初始化
            foreach (UILoadingInfo uiLoadingInfo in loadingList)
			{
				if(string.IsNullOrEmpty(uiLoadingInfo.addressNameStr))
				{
					continue;
				}

				++sumOfLoading;

                AsyncOperationHandle<GameObject> uiAssetHandle	= Addressables.LoadAssetAsync<GameObject>(uiLoadingInfo.addressNameStr);
                uiAssetHandle.Completed							+= (resultHandle) => 
				{
					uiLoadingInfo.uiPerfab = resultHandle.Result;
					++currLoading;

					if(currLoading == sumOfLoading)
					{
                        UIController.Instance.LoadingUI(this);
					}
                };
            }
#endif //ADDRESSABLES_EXISTS

            if (sumOfLoading == 0)
			{
                UIController.Instance.LoadingUI(this);
            }
        }

        private void OnApplicationQuit()
		{
			bCancalUnload = true;
		}

		private void OnDestroy()
		{
			// 避免在destroy的時候new 任何東西
			if(bCancalUnload)
			{
				return;
			}

			UIController.Instance.UnloadingUI(this);
		}

		public List<UILoadingInfo> GetLoadingList()
		{
            // 初始化 UILoadingInfo 的 uiList
            foreach (UILoadingInfo uiLoadingInfo in loadingList)
            {
                if(uiLoadingInfo.uiList == null || uiLoadingInfo.uiList.Length == 0)
                {
                    uiLoadingInfo.uiList = uiLoadingInfo.uiPerfab.GetComponents<UIBase>();
                }                
            }

			return loadingList;
		}

		public bool NeedToDestroyOtherUI()
		{
			return bDestroyOtherUI;
		}
	}
}
