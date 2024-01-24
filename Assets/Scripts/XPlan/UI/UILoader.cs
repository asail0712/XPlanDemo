using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace XPlan.UI
{
	[Serializable]
	public struct UILoadingInfo
	{
		[SerializeField]
		public GameObject uiGO;

		[SerializeField]
		public int sortIdx;

		[SerializeField]
		public bool bIsPersistentUI;
	}

    public class UILoader : MonoBehaviour
    {
		[SerializeField]
		List<UILoadingInfo> loadingList = new List<UILoadingInfo>();

		[SerializeField]
		bool bDontDestroyOtherUI		= false;

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
			return loadingList;
		}

		public bool NeedToDestroyOtherUI()
		{
			return !bDontDestroyOtherUI;
		}
	}
}
