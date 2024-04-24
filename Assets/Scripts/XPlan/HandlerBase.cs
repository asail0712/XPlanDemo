using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XPlan.Interface;
using XPlan.Observe;
using XPlan.UI;
using XPlan.Utility;

namespace XPlan
{
	public class HandlerBase : IUIListener, INotifyReceiver
	{
		private List<MonoBehaviourHelper.MonoBehavourInstance> coroutineList;

		/*************************
		 * Coroutine相關
		 * ***********************/
		protected MonoBehaviourHelper.MonoBehavourInstance StartCoroutine(IEnumerator routine, bool persistent = false)
		{
			MonoBehaviourHelper.MonoBehavourInstance coroutine = MonoBehaviourHelper.StartCoroutine(routine, persistent);

			coroutineList.Add(coroutine);

			return coroutine;
		}

		protected void StopCoroutine(MonoBehaviourHelper.MonoBehavourInstance coroutine)
		{
			coroutine.StopCoroutine();

			coroutineList.Remove(coroutine);
		}

		/*************************
		 * Notify相關
		 * ***********************/
		protected void RegisterNotify<T>(INotifyReceiver notifyReceiver, Action<T> notifyAction) where T : MessageBase
		{
			NotifySystem.Instance.RegisterNotify<T>(notifyReceiver, (msgReceiver) =>
			{
				T msg = msgReceiver.GetMessage<T>();

				notifyAction?.Invoke(msg);
			});
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
			UISystem.RegisterCallback(uniqueID, this, (param)=> 
			{
				callback?.Invoke(param.GetValue<T>());
			});
		}

		protected void AddUIListener(string uniqueID, Action callback)
		{
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
		public HandlerBase()
		{
			coroutineList = new List<MonoBehaviourHelper.MonoBehavourInstance>();
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
			foreach(MonoBehaviourHelper.MonoBehavourInstance coroutine in coroutineList)
			{
				if(coroutine != null)
				{
					coroutine.StopCoroutine();
				}
			}
			coroutineList.Clear();

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

	}
}

