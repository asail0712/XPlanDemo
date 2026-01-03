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
using UnityEngine.UI;
using UnityEngine.EventSystems;

using XPlan.Interface;

namespace XPlan.UI.Components
{
	public class GdScrollView : ScrollRect
	{
		public Action<PointerEventData> beginDragDelegate, dragingDelgate, endDragDelegate, potentialDragDelegate, onScrollDelegate;

		public override void OnBeginDrag(PointerEventData eventData)
		{
			beginDragDelegate?.Invoke(eventData);
		}
		public override void OnDrag(PointerEventData eventData)
		{
			dragingDelgate?.Invoke(eventData);
		}
		public override void OnEndDrag(PointerEventData eventData)
		{
			endDragDelegate?.Invoke(eventData);
		}
		public override void OnInitializePotentialDrag(PointerEventData eventData)
		{
			potentialDragDelegate?.Invoke(eventData);
		}
		public override void OnScroll(PointerEventData eventData)
		{
			onScrollDelegate?.Invoke(eventData);
		}

		protected override void OnEnable()
		{
			onValueChanged.AddListener(OnScrollChange);
		}

		protected override void OnDisable()
		{
			onValueChanged.RemoveListener(OnScrollChange);
		}

		private void OnScrollChange(Vector2 pos)
		{
			IScrollItem[] itemList = this.content.GetComponentsInChildren<IScrollItem>();

			if(itemList.Length == 0)
			{
				return;
			}

			Array.ForEach(itemList, (X)=> 
			{
				X.SetContentPos(content.anchoredPosition);
			});
		}
	}
}

