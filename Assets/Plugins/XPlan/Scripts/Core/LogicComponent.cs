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

			if(coroutine != null)
            {
				coroutine.StopCoroutine();
			}

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
			T msg = (T)ctor.Invoke(args);
			msg.Send();
		}

        protected void SendMsgAsync<T>(params object[] args) where T : MessageBase
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
            string groupID = GetLazyZoneID?.Invoke();
            T msg = (T)ctor.Invoke(args);
            msg.Send(true);
        }

        /*************************
		 * ServiceLocator相關
		 * ***********************/
		protected T GetService<T>() where T : class
        {
            return ServiceLocator.GetService<T>();
        }

        protected bool TryGetService<T>(out T service) where T : class
        {
			if(!ServiceLocator.HasService<T>())
			{
				service = null;
                return false;
			}

            service = ServiceLocator.GetService<T>();
			return true;
        }

        /*************************
		 * UI相關
		 * ***********************/
        protected void DirectCallUI<T>(string uniqueID, T value)
		{
			UIEventBus.DirectCall<T>(uniqueID, value);
		}

		protected void DirectCallUI(string uniqueID)
		{
			UIEventBus.DirectCall(uniqueID);
		}

		protected void DirectCallUI(string uniqueID, params object[] paramList)
		{
			UIEventBus.DirectCall(uniqueID, paramList);
		}

		protected void AddUIListener<T>(string uniqueID, Action<T> callback)
		{
			if(!bEnabled)
			{
				return;
			}

			UIEventBus.RegisterCallback(uniqueID, this, (param)=> 
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

			UIEventBus.RegisterCallback(uniqueID, this, (dump) =>
			{
				callback?.Invoke();
			});
		}

		protected void RemoveUIListener(string uniqueID)
		{
			UIEventBus.UnregisterCallback(uniqueID, this);
		}

		protected void RemoveAllUIListener()
		{
			UIEventBus.UnregisterAllCallback(this);
		}

        /********************************
		 * 其他
		 * *****************************/
        protected string GetStr(string keyStr)
        {
            return StringTable.Instance.GetStr(keyStr);
        }

        protected string ReplaceStr(string keyStr, params string[] paramList)
        {
            return StringTable.Instance.ReplaceStr(keyStr, paramList);
        }

        /*************************
		 * 初始化與釋放
		 * ***********************/
        public LogicComponent()
		{
			coroutineDict = new Dictionary<int, MonoBehaviourHelper.MonoBehavourInstance>();

            // 建構完成後，嘗試呼叫 IL Weaving 產生的 Hook
            InvokeWeaverHook();
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
		 * Hook
		 * ***********************/
        /// <summary>
        /// 給 IL Weaving 的 Hook 入口：
        /// 在衍生類別中產生一個：
        ///   void __LogicComponent_WeaverHook()
        /// 就會被這裡自動呼叫
        /// </summary>
        private void InvokeWeaverHook()
        {
            const string HookMethodName = "__LogicComponent_WeaverHook";

            // 取得實際執行個體的型別（衍生類別）
            var type	= GetType();

            var method	= type.GetMethod(
                HookMethodName,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            if (method == null)
                return;

            // 安全限制：必須是 void、無參數
            if (method.ReturnType != typeof(void))
                return;

            if (method.GetParameters().Length != 0)
                return;

            method.Invoke(this, null);
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

