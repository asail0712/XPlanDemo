using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

using XPlan.DebugMode;
using XPlan.Interface;
using XPlan.Observe;
using XPlan.UI;
using XPlan.Utility;
using XPlan.Weaver.Runtime;

namespace XPlan
{
	public class LogicComponent : IUIListener, INotifyReceiver
	{
		private Dictionary<int, MonoBehaviourHelper.MonoBehavourInstance> coroutineDict;
		private static int corourintSerialNum	= 0;

        /*************************
		 * 初始化與釋放
		 * ***********************/
        public LogicComponent()
        {
            coroutineDict = new Dictionary<int, MonoBehaviourHelper.MonoBehavourInstance>();

            // 建構完成後，嘗試呼叫 IL Weaving 產生的 Hook
            WeaverHookInvoker.Invoke(this, "__LogicComponent_WeaverHook");
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
            foreach (KeyValuePair<int, MonoBehaviourHelper.MonoBehavourInstance> kvp in coroutineDict)
            {
                MonoBehaviourHelper.MonoBehavourInstance coroutine = kvp.Value;

                if (coroutine != null)
                {
                    coroutine.StopCoroutine();
                }
            }

            coroutineDict.Clear();

            if (!bAppQuit)
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
            NotifyHelper.RegisterNotify<T>(this, notifyAction);
        }

		protected void RegisterNotify<T>(ReceiveOption option, Action<T> notifyAction) where T : MessageBase
		{
			NotifyHelper.RegisterNotify<T>(this, option, notifyAction);
		}

		protected void SendMsg<T>(params object[] args) where T : MessageBase
		{
            NotifyHelper.SendMsg<T>(false, args);
        }

        protected void SendMsgAsync<T>(params object[] args) where T : MessageBase
        {
			NotifyHelper.SendMsg<T>(true, args);
        }

        public Task<TResult> SendMsg<TMsg, TResult>(params object[] args) where TMsg : MessageBase<TResult>
        {
            var tcs		= new TaskCompletionSource<TResult>();
            Type type	= typeof(TMsg);

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
            TMsg msg			= (TMsg)ctor.Invoke(args);
			msg.finishAction	= (result) =>
			{
                tcs.TrySetResult(result);
			};

            msg.Send(true);

            return tcs.Task;
        }

        public Task SendAsyncMsg<TMsg>(params object[] args) where TMsg : MessageWithRet
        {
            var tcs     = new TaskCompletionSource<bool>();
            Type type   = typeof(TMsg);

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
            TMsg msg            = (TMsg)ctor.Invoke(args);
            msg.finishAction    = () =>
            {
                tcs.TrySetResult(true);
            };

            msg.Send(true);

            return tcs.Task;
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
			UIEventBus.RegisterCallback(uniqueID, this, (param)=> 
			{
				callback?.Invoke(param.GetValue<T>());
			});
		}

		protected void AddUIListener(string uniqueID, Action callback)
		{
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
	}
}

