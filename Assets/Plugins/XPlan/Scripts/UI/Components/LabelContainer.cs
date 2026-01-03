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
using UnityEngine;

namespace XPlan.UI.Components
{
	[Serializable]
	public class LabelContainer
	{
		private LabelButton[] labelList;
		private int currChoose;

		public LabelContainer(LabelButton[] labelBtnList, Action<int> onLabelClick = null, int idx = 0)
		{
			this.labelList	= labelBtnList;
			this.currChoose = idx;

			// 確認長度是否足夠
			if(labelList.Length <= currChoose)
			{
				Debug.LogError("LabelContainer的Label list長度不夠 !!");
			}

			// 設定初始label 與 加掛delegate
			Array.ForEach(labelList, X => 
			{
				// 加掛Delegate
				X.onClickLabel += (labelIdx) => 
				{
					currChoose = labelIdx;

					// 更新所有Label狀態
					Array.ForEach(labelList, Y =>
					{
						Y.SwitchLabel(Y.labelIndex == currChoose);
					});

					onLabelClick?.Invoke(labelIdx);
				};

				// 更新初始Label狀態
				X.SwitchLabel(X.labelIndex == currChoose);
			});

			onLabelClick?.Invoke(currChoose);
		}
	}
}

