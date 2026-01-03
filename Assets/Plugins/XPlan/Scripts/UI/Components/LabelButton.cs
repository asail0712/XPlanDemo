// ==============================================================================
// XPlan Framework
//
// Copyright (c) 2026 Asail
// All rights reserved.
//
// Author  : Asail0712
// Project : XPlan
// Description:
//     A modular framework for Unity projects, focusing on MVVM architecture,
//     runtime tooling, event-driven design, and extensibility.
//
// Contact : asail0712@gmail.com
// GitHub  : https://github.com/asail0712/XPlanDemo
//
// Unauthorized copying, modification, or distribution of this file,
// via any medium, is strictly prohibited without prior permission.
// ==============================================================================
using System;

namespace XPlan.UI.Components
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

