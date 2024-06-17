using System;
using System.Collections.Generic;
using UnityEngine;

using XPlan.Interface;
using XPlan.Utility;

namespace XPlan
{
	public class HandlerInfo
	{
		public HandlerBase handler;
		public InstallerBase container;
		public Type handlerType;
	}

	public class HandlerManager
	{
        private Dictionary<string, HandlerInfo> handlerDict	= new Dictionary<string, HandlerInfo>();

		/************************************
		* 初始化
		* **********************************/

		public void RegisterScope(HandlerBase handler, InstallerBase container)
		{
			string key = GetKey(handler, container);

			handlerDict.Add(key, new HandlerInfo() 
			{
				handler		= handler,
				container	= container,
				handlerType = handler.GetType(),
			});
		}
		public void UnregisterScope(HandlerBase handler, InstallerBase container)
		{
			if(null == handler)
			{
				return;
			}

			handler.Dispose(false);

			string key = GetKey(handler, container);

			handlerDict.Remove(key);
		}

		public void UnregisterScope(InstallerBase container, bool bAppQuit)
		{
			List<string> disposeList = new List<string>();

			foreach (var kvp in handlerDict)
			{
				if(kvp.Value.container == container)
				{
					kvp.Value.handler.Dispose(bAppQuit);

					disposeList.Add(kvp.Key);
				}				
			}

			disposeList.ForEach((key) => 
			{
				handlerDict.Remove(key);
			});
		}

		public void PostInitial()
		{
			foreach (var kvp in handlerDict)
			{
				HandlerBase handler = kvp.Value.handler;

				handler.PostInitial();
			}
		}

		public void TickHandler(float deltaTime)
		{
			//Debug.Log($"HandlerManager Update !!");

			foreach (var kvp in handlerDict)
			{
				HandlerBase handler = kvp.Value.handler;

				if (handler is ITickable)
				{
					ITickable tickable = (ITickable)handler;

					tickable.Tick(deltaTime);
				}
			}
		}

		private string GetKey(HandlerBase handler, InstallerBase container)
		{
			string key1			= handler.GetType().ToString();
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
