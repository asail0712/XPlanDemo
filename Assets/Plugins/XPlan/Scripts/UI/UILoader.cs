using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace XPlan.UI
{
	[Serializable]
	public class UILoadingInfo
	{
		[SerializeField]
		public GameObject uiPerfab;

		[SerializeField]
		public int rootIdx;

		[SerializeField]
		public int sortIdx;

		[SerializeField]
		public bool bIsPersistentUI;

		[SerializeField]
		public bool bVisible;

        [HideInInspector]
        public UIBase[] uiList;

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
		[SerializeField] List<UILoadingInfo> loadingList	= new List<UILoadingInfo>();
		[SerializeField] bool bDestroyOtherUI				= false;

		private bool bCancalUnload		= false;

		private void Awake()
		{			
			UIController.Instance.LoadingUI(this);
		}

		void OnApplicationQuit()
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
