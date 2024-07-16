using System;
using System.Collections.Generic;
using UnityEngine;

using XPlan.Interface;
using XPlan.Utility;

namespace XPlan
{
	public class LogicComponentInfo
	{
		public LogicComponentBase logicComp;
		public SystemBase container;
		public Type handlerType;
	}

	public class LogicManager
	{
        private Dictionary<string, LogicComponentInfo> logicDict	= new Dictionary<string, LogicComponentInfo>();

		/************************************
		* 初始化
		* **********************************/

		public void RegisterScope(LogicComponentBase logicComp, SystemBase container)
		{
			string key = GetKey(logicComp, container);

			logicDict.Add(key, new LogicComponentInfo() 
			{
				logicComp	= logicComp,
				container	= container,
				handlerType = logicComp.GetType(),
			});
		}
		public void UnregisterScope(LogicComponentBase logicComp, SystemBase container)
		{
			if(null == logicComp)
			{
				return;
			}

			logicComp.Dispose(false);

			string key = GetKey(logicComp, container);

			logicDict.Remove(key);
		}

		public void UnregisterScope(SystemBase container, bool bAppQuit)
		{
			List<string> disposeList = new List<string>();

			foreach (var kvp in logicDict)
			{
				if(kvp.Value.container == container)
				{
					kvp.Value.logicComp.Dispose(bAppQuit);

					disposeList.Add(kvp.Key);
				}				
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
				LogicComponentBase handler = kvp.Value.logicComp;

				handler.PostInitial();
			}
		}

		public void TickLogic(float deltaTime)
		{
			foreach (var kvp in logicDict)
			{
				LogicComponentBase handler = kvp.Value.logicComp;

				if (handler is ITickable)
				{
					ITickable tickable = (ITickable)handler;

					tickable.Tick(deltaTime);
				}
			}
		}

		private string GetKey(LogicComponentBase logicComp, SystemBase container)
		{
			string key1			= logicComp.GetType().ToString();
			int lastDotIndex1	= key1.LastIndexOf('.'); // 找到最後一個小數點的索引

			if(lastDotIndex1 != -1)
			{
				key1 = key1.Substring(lastDotIndex1 + 1);
			}

			string key2			= container.GetType().ToString();
			int lastDotIndex2	= key2.LastIndexOf('.'); // 找到最後一個小數點的索引

			if (lastDotIndex2 != -1)
			{
				key2 = key2.Substring(lastDotIndex2 + 1);
			}

			return key1 + "_" + key2;
		}
	}
}
