using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

using XPlan.DebugMode;
using XPlan.Interface;
using XPlan.Observe;
using XPlan.UI;
using XPlan.Utility;

namespace XPlan
{
	public class LogicSwitcher : LogicComponent
	{
		public Action<Type, bool> switchLogicAction;

		protected void SwitchLogic<T>(bool bEnable)
		{
			switchLogicAction?.Invoke(typeof(T), bEnable);
		}
	}
}

