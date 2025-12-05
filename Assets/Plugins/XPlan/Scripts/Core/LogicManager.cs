using System;
using System.Collections.Generic;
using UnityEngine;

using XPlan.Interface;
using XPlan.Utility;

namespace XPlan
{
	public class LogicComponentInfo
	{
		public LogicComponent logicComp;
		public Type logicType;
	}

	public class LogicManager
	{
        private Dictionary<Type, LogicComponentInfo> logicDict = new Dictionary<Type, LogicComponentInfo>();

		/************************************
		* 初始化
		* **********************************/

		public void RegisterScope(LogicComponent logicComp)
		{
			Type logicType = logicComp.GetType();

			logicDict.Add(logicType, new LogicComponentInfo() 
			{
				logicComp	= logicComp,
				logicType	= logicComp.GetType(),
			});

			if(logicComp is LogicSwitcher)
			{
				LogicSwitcher logicSwitcher = logicComp as LogicSwitcher;

				if(logicSwitcher != null)
				{
					logicSwitcher.switchLogicAction = SwitchLogic;
				}
			}
		}
		public void UnregisterScope(LogicComponent logicComp)
		{
			if(null == logicComp)
			{
				return;
			}

			logicComp.Dispose(false);
			
			logicDict.Remove(logicComp.GetType());
		}

		public void UnregisterScope(bool bAppQuit)
		{
			List<Type> disposeList = new List<Type>();

			foreach (var kvp in logicDict)
			{
				kvp.Value.logicComp.Dispose(bAppQuit);

				disposeList.Add(kvp.Key);
			}

			disposeList.ForEach((key) => 
			{
				logicDict.Remove(key);
			});
		}

		public void PostInitial()
		{
			foreach (var kvp in logicDict)
			{
				LogicComponent logicComp = kvp.Value.logicComp;

				logicComp.PostInitial();
			}
		}

		public void TickLogic(float deltaTime)
		{
			foreach (var kvp in logicDict)
			{
				LogicComponent logicComp = kvp.Value.logicComp;

				if (logicComp is ITickable && logicComp.IsEnabled())
				{
					ITickable tickable = (ITickable)logicComp;

					tickable.Tick(deltaTime);
				}
			}
		}

		/************************************
		* 其他
		* **********************************/
		private void SwitchLogic(Type type, bool bEnabled)
		{
			if(!logicDict.ContainsKey(type))
			{
				return;
			}

			logicDict[type].logicComp.SwitchLogic(bEnabled);
		}
	}
}
