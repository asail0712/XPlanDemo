using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using XPlan.UI;
using XPlan.UI.Component;

namespace XPlan.DebugMode
{
	public class DebugPanel : UIBase
	{
		[SerializeField]
		private Toggle[] toggleArr;

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
			if(contentList.Length != toggleArr.Length)
			{
				Debug.LogWarning("偵錯面板設定有誤 !!");
			}

			/*******************************
			 * 使用者操作
			 * *****************************/
			RegisterToggles("", toggleArr, (idx)=> 
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

			/*******************************
			 * 收到命令
			 * *****************************/
			ListenCall(ShowDebugPanel, () => 
			{
				uiRoot.SetActive(true);
			});
		}
	}
}
