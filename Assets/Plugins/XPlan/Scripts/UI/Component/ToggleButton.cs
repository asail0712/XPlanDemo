using System;
using UnityEngine;
using UnityEngine.UI;

using XPlan.Interface;

namespace XPlan.UI.Component
{
	[Serializable]
	public class ToggleButton : IToggleButton
	{
		[SerializeField]
		private Button toggleBtn;

		[SerializeField]
		private GameObject activateIcon;

		[SerializeField]
		private GameObject deactivateIcon;

		private bool bIsPress;

		/********************************************
		 * IButton介面實作
		 * ******************************************/
		public event Action onClick;

		public ToggleButton(Button btn, GameObject enableIcon, GameObject disableIcon)
		{
			this.toggleBtn		= btn;
			this.activateIcon	= enableIcon;
			this.deactivateIcon = disableIcon;

			if(toggleBtn != null)
			{
				toggleBtn.onClick.RemoveAllListeners();
				toggleBtn.onClick.AddListener(OnClickButton);
			}			
		}

		public void Toggle()
		{
			Switch(!bIsPress);
		}

		public void Switch(bool bEnable)
		{
			if(bEnable)
			{
				PressBtn();
			}
			else
			{
				ReleaseBtn();
			}
		}

		public void PressBtn()
		{
			bIsPress = true;
			RefreshUI();
		}

		public void ReleaseBtn()
		{
			bIsPress = false;
			RefreshUI();
		}

		private void RefreshUI()
		{
			if (activateIcon == null || deactivateIcon == null)
			{
				return;
			}

			activateIcon.SetActive(bIsPress);
			deactivateIcon.SetActive(!bIsPress);
		}

		private void OnClickButton()
		{
			if(bIsPress)
			{
				return;
			}

			Toggle();

			onClick?.Invoke();
		}

	}
}

