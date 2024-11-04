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
	public class LogicComponent : IUIListener, INotifyReceiver
	{
		private Dictionary<int, MonoBehaviourHelper.MonoBehavourInstance> coroutineDict;
		private static int corourintSerialNum	= 0;
		private bool bEnabled					= true;

		/*************************
		 * 實作 INotifyReceiver
		 * ***********************/
		public Func<string> GetLazyZoneID { get; set; }

		/*************************
		 * Coroutine相關
		 * ***********************/
		protected int StartCoroutine(IEnumerator routine, bool persistent = false)
		{
			// 清除已經停止的Coroutine
			ClearCoroutine();

			MonoBehaviourHelper.MonoBehavourInstance coroutine = MonoBehaviourHelper.StartCoroutine(routine, persistent);

			coroutineDict.Add(++corourintSerialNum, coroutine);

			return corourintSerialNum;
		}

		protected void StopCoroutine(int serialNum)
		{
			if(!coroutineDict.ContainsKey(serialNum))
			{
				return;
			}

			MonoBehaviourHelper.MonoBehavourInstance coroutine = coroutineDict[serialNum];

			coroutine.StopCoroutine();

			coroutineDict.Remove(serialNum);
		}

		protected bool IsCoroutineRunning(int serialNum)
		{
			if (coroutineDict.ContainsKey(serialNum))
			{
				return coroutineDict[serialNum] != null;
			}

			return false;
		}

		private void ClearCoroutine()
		{
			List<int> serialNumList = new List<int>();

			foreach (KeyValuePair<int, MonoBehaviourHelper.MonoBehavourInstance> kvp in coroutineDict)
			{
				if(kvp.Value == null)
				{
					serialNumList.Add(kvp.Key);
				}
			}

			foreach(int serialNum in serialNumList)
			{
				coroutineDict.Remove(serialNum);
			}
		}

		/*************************
		 * Notify相關
		 * ***********************/
		protected void RegisterNotify<T>(Action<T> notifyAction) where T : MessageBase
		{
			if (!bEnabled)
			{
				return;
			}

			INotifyReceiver notifyReceiver = this as INotifyReceiver;

			if(notifyReceiver == null)
			{
				LogSystem.Record($"{this} is not implement INotifyReceiver", LogType.Error);
				return;
			}

			NotifySystem.Instance.RegisterNotify<T>(notifyReceiver, (msgReceiver) =>
			{
				T msg = msgReceiver.GetMessage<T>();

				notifyAction?.Invoke(msg);
			});
		}

		protected void RegisterNotify<T>(ReceiveOption option, Action<T> notifyAction) where T : MessageBase
		{
			if (!bEnabled)
			{
				return;
			}

			INotifyReceiver notifyReceiver = this as INotifyReceiver;

			if (notifyReceiver == null)
			{
				LogSystem.Record($"{this} is not implement INotifyReceiver", LogType.Error);
				return;
			}

			NotifySystem.Instance.RegisterNotify<T>(notifyReceiver, option, (msgReceiver) =>
			{
				T msg = msgReceiver.GetMessage<T>();

				notifyAction?.Invoke(msg);
			});
		}

		protected void SendMsg<T>(params object[] args) where T : MessageBase
		{
			// 获取类型
			Type type = typeof(T);

			// 查找匹配的构造函数
			ConstructorInfo ctor = type.GetConstructor(
				BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
				null,
				CallingConventions.HasThis,
				Array.ConvertAll(args, item => item.GetType()),
				null
			);

			if (ctor == null)
			{
				throw new Exception($"No matching constructor found for {type.Name}");
			}

			// 生成msg並寄出
			string groupID	= GetLazyZoneID?.Invoke();
			T msg			= (T)ctor.Invoke(args);
			msg.Send(groupID);
		}

		protected void SendGlobalMsg<T>(params object[] args) where T : MessageBase
		{
			// 获取类型
			Type type = typeof(T);

			// 查找匹配的构造函数
			ConstructorInfo ctor = type.GetConstructor(
				BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
				null,
				CallingConventions.HasThis,
				Array.ConvertAll(args, item => item.GetType()),
				null
			);

			if (ctor == null)
			{
				throw new Exception($"No matching constructor found for {type.Name}");
			}

			// 生成msg並寄出
			T msg = (T)ctor.Invoke(args);
			msg.Send("");
		}

		protected void SendMsgWithGroup<T>(string groupID, params object[] args) where T : MessageBase
		{
			// 获取类型
			Type type = typeof(T);

			// 查找匹配的构造函数
			ConstructorInfo ctor = type.GetConstructor(
				BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
				null,
				CallingConventions.HasThis,
				Array.ConvertAll(args, item => item.GetType()),
				null
			);

			if (ctor == null)
			{
				throw new Exception($"No matching constructor found for {type.Name}");
			}

			// 生成msg並寄出
			T msg = (T)ctor.Invoke(args);
			msg.Send(groupID);
		}

		/*************************
		 * UI相關
		 * ***********************/
		protected void DirectCallUI<T>(string uniqueID, T value)
		{
			UISystem.DirectCall<T>(uniqueID, value);
		}

		protected void DirectCallUI(string uniqueID)
		{
			UISystem.DirectCall(uniqueID);
		}

		protected void DirectCallUI(string uniqueID, params object[] paramList)
		{
			UISystem.DirectCall(uniqueID, paramList);
		}

		protected void AddUIListener<T>(string uniqueID, Action<T> callback)
		{
			if(!bEnabled)
			{
				return;
			}

			UISystem.RegisterCallback(uniqueID, this, (param)=> 
			{
				callback?.Invoke(param.GetValue<T>());
			});
		}

		protected void AddUIListener(string uniqueID, Action callback)
		{
			if (!bEnabled)
			{
				return;
			}

			UISystem.RegisterCallback(uniqueID, this, (dump) =>
			{
				callback?.Invoke();
			});
		}

		protected void RemoveUIListener(string uniqueID)
		{
			UISystem.UnregisterCallback(uniqueID, this);
		}

		protected void RemoveAllUIListener()
		{
			UISystem.UnregisterAllCallback(this);
		}

		/*************************
		 * 初始化與釋放
		 * ***********************/
		public LogicComponent()
		{
			coroutineDict = new Dictionary<int, MonoBehaviourHelper.MonoBehavourInstance>();
		}

		public void PostInitial()
		{
			OnPostInitial();
		}

		protected virtual void OnPostInitial()
		{
			// for override
		}

		public void Dispose(bool bAppQuit)
		{
			// 清除ui listener
			RemoveAllUIListener();

			// 清除coroutine
			foreach(KeyValuePair<int, MonoBehaviourHelper.MonoBehavourInstance> kvp in coroutineDict)
			{
				MonoBehaviourHelper.MonoBehavourInstance coroutine = kvp.Value;

				if (coroutine != null)
				{
					coroutine.StopCoroutine();
				}
			}

			coroutineDict.Clear();

			if(!bAppQuit)
			{ 
				// 清除notify
				NotifySystem.Instance.UnregisterNotify(this);
			}

			OnDispose(bAppQuit);
		}

		protected virtual void OnDispose(bool bAppQuit)
		{
			// for override
		}

		/*************************
		 * Enabled相關
		 * ***********************/
		public void SwitchLogic(bool bEnabled)
		{
			this.bEnabled = bEnabled;

			if(!bEnabled)
			{
				coroutineDict.Clear();
			}
		}

		public bool IsEnabled()
		{
			return bEnabled;
		}
	}
}

