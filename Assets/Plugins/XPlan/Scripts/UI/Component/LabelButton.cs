using System;

namespace XPlan.UI.Component
{
	[Serializable]
	public class LabelButton
	{
		private ToggleButton toggleButton;
		private int labelIdx;

		public int labelIndex { get => labelIdx; }
		public event Action<int> onClickLabel;

		public LabelButton(ToggleButton toggleBtn, int idx)
		{
			this.toggleButton	= toggleBtn;
			this.labelIdx		= idx;

			if (toggleBtn != null)
			{
				toggleButton.onClick += OnClickButton;
			}			
		}

		public void SwitchLabel(bool b)
		{
			toggleButton.Switch(b);
		}

		private void OnClickButton()
		{
			onClickLabel?.Invoke(labelIdx);
		}

	}
}

