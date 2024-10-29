using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using XPlan.Utility;

namespace XPlan.UI.Template
{
	public enum DialogResult
	{
		Confirm = 0,
		Cancal,
	}

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
		public Action<DialogResult> clickAction	= null;

		public ShowDialogue(DialogType dialogType, string showStr, Action<DialogResult> clickAction = null, string confirmKey = "", string cancelKey = "")
		{
			Initial(dialogType, showStr, clickAction, confirmKey, cancelKey);
		}
		
		private void Initial(DialogType dialogType, string showStr, Action<DialogResult> clickAction, string confirmKey = "", string cancelKey = "")
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
			this.showStr		= showStr;
			this.confirmStr		= confirmKey;
			this.cancelStr		= cancelKey;
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

		private Action<DialogResult> clickAction;

		private void Awake()
		{
			RegisterButton("", confirmBtn, () =>
			{
				clickAction?.Invoke(DialogResult.Confirm);
				uiRoot.SetActive(false);
			});

			RegisterButton("", cancelBtn, () =>
			{
				clickAction?.Invoke(DialogResult.Cancal);
				uiRoot.SetActive(false);
			});

			ListenCall<ShowDialogue>(DialogMessage.ConfirmMessage, (info)=> 
			{
				showStrTxt.text = GetStr(info.showStr);
				confirmTxt.text = GetStr(info.confirmStr);
				cancelTxt.text	= GetStr(info.cancelStr);
				clickAction		= info.clickAction;

				cancelBtn.gameObject.SetActive(info.dialogType == DialogType.DualButton);

				uiRoot.SetActive(true);
			});
		}
	}
}
