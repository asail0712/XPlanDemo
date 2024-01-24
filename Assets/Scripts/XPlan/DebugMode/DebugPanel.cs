using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using XPlan.UI;

namespace XPlan.DebugMode
{
	public class DebugPanel : UIBase
	{
		[SerializeField]
		private UILabel[] labelList;

		[SerializeField]
		private GameObject[] contentList;

		[SerializeField]
		private GameObject uiRoot;

		[SerializeField]
		private Button exitBtn;

		private const string ShowDebugPanel = "ShowDebugPanel";
		private const string HideDebugPanel = "HideDebugPanel";

		protected void Awake()
		{
			if(contentList.Length != labelList.Length)
			{
				Debug.LogWarning("偵錯面板設定有誤 !!");
			}

			RegisterLabels("", labelList, (idx)=> 
			{
				for(int i = 0; i < contentList.Length; ++i)
				{
					contentList[i].SetActive(i == idx);
				}
			});

			RegisterButton(HideDebugPanel, exitBtn, () =>
			{
				uiRoot.SetActive(false);
			});

			ListenCall(ShowDebugPanel);
		}

		protected override void OnNotifyUI(string uniqueID, params UIParam[] value)
		{
			switch(uniqueID)
			{
				case ShowDebugPanel:
					uiRoot.SetActive(true);
					break;
			}
		}

	}
}
