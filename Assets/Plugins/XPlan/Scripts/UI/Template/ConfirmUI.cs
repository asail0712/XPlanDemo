using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using XPlan.Utility;

namespace XPlan.UI.Template
{
	public static class DialogMessage
	{
		public const string Confirm			= "key_confirm";
		public const string Cancel			= "key_cancel";
		public const string ConfirmMessage	= "ConfirmMessage";
	}

	public enum DialogType
	{
		SingleButton,
		DualButton
	}

	public class ShowDialogue
	{
		public DialogType dialogType	= DialogType.SingleButton;
		public string showStr			= "";
		public string confirmStr		= "";
		public string cancelStr			= "";
		public Action<int> clickAction	= null;

		public ShowDialogue(DialogType dialogType, string showStr, Action<int> clickAction = null, string confirmKey = "", string cancelKey = "")
		{
			Initial(dialogType, showStr, clickAction, confirmKey, cancelKey);
		}
		
		public ShowDialogue(DialogType dialogType, string[] showStrList, Action<int> clickAction = null, string confirmKey = "", string cancelKey = "")
		{			
			string showStr = "";

			for (int i = 0; i < showStrList.Length; ++i)
			{
				showStr += StringTable.Instance.GetStr(showStrList[i]);
			}

			Initial(dialogType, showStr, clickAction, confirmKey, cancelKey);
		}

		private void Initial(DialogType dialogType, string showStr, Action<int> clickAction, string confirmKey = "", string cancelKey = "")
		{
			if (confirmKey == "")
			{
				confirmKey = DialogMessage.Confirm;
			}

			if (cancelKey == "")
			{
				cancelKey = DialogMessage.Cancel;
			}


			this.dialogType		= dialogType;
			this.showStr		= StringTable.Instance.GetStr(showStr);
			this.confirmStr		= StringTable.Instance.GetStr(confirmKey);
			this.cancelStr		= StringTable.Instance.GetStr(cancelKey);
			this.clickAction	= clickAction;

			UISystem.DirectCall<ShowDialogue>(DialogMessage.ConfirmMessage, this);
		}
	}

	public class ConfirmUI : UIBase
    {
		[SerializeField] public Button confirmBtn;
		[SerializeField] public Button cancelBtn;
		[SerializeField] public GameObject uiRoot;
		[SerializeField] public Text showStrTxt;
		[SerializeField] public Text confirmTxt;
		[SerializeField] public Text cancelTxt;

		private Action<int> clickAction;

		private void Awake()
		{
			RegisterButton("", confirmBtn, () =>
			{
				clickAction?.Invoke(0);
				uiRoot.SetActive(false);
			});

			RegisterButton("", cancelBtn, () =>
			{
				clickAction?.Invoke(1);
				uiRoot.SetActive(false);
			});

			ListenCall<ShowDialogue>(DialogMessage.ConfirmMessage, (info)=> 
			{
				showStrTxt.text = info.showStr;
				confirmTxt.text = info.confirmStr;
				cancelTxt.text	= info.cancelStr;
				clickAction		= info.clickAction;

				cancelBtn.gameObject.SetActive(info.dialogType == DialogType.DualButton);

				uiRoot.SetActive(true);
			});
		}
	}
}
