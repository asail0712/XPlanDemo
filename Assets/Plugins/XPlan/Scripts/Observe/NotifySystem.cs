using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XPlan.DebugMode;
using XPlan.Utility;
using XPlan.Extensions;

namespace XPlan.Observe
{
	public class MessageBase
	{
		public void Send(string groupID = "")
		{
			MessageSender sender = new MessageSender(this);

			sender.SendMessage(groupID);
		}
	}

	public class ReceiveOption
	{
		// 相依性
		public List<Type> dependOnList;
	}

	public class ActionInfo
	{
		public INotifyReceiver notifyReceiver;
		public ReceiveOption receiveOption;
		public Action<MessageReceiver> receiverAction;
	}

	public class MessageReceiver
	{
		private MessageSender msgSender;

		public MessageReceiver(MessageSender msgSender)
		{
			this.msgSender = msgSender;
		}

		public bool CorrespondType(Type type)
		{
			return msgSender.GetType() == type;
		}

		public bool CorrespondType<T>()
		{
			return msgSender.msg is T;
		}

		public T GetMessage<T>() where T : MessageBase
		{
#if DEBUG
			string className	= msgSender.stackInfo.GetClassName();
			string methodName	= msgSender.stackInfo.GetMethodName();
			string lineNumber	= msgSender.stackInfo.GetLineNumber();
			string fullLogInfo	= $"Notify({msgSender.msg.GetType()}) from [ {className}::{methodName}() ], line {lineNumber} ";

			LogSystem.Record(fullLogInfo);
#endif //DEBUG

			return (T)(msgSender.msg);
		}
	}

	public class MessageSender
	{
		public MessageBase msg;
#if DEBUG
		public StackInfo stackInfo;
#endif //DEBUG

		public MessageSender(MessageBase msg)
		{
			this.msg		= msg;
#if DEBUG
			this.stackInfo	= new StackInfo(4);
#endif //DEBUG
		}

		public void SendMessage(string groupID)
		{
			NotifySystem.Instance.SendMsg(this, groupID);
		}

		public Type GetMsgType()
		{
			return msg.GetType();
		}
	}

	public class NotifyInfo
	{
		public INotifyReceiver notifyReceiver;
		public Dictionary<Type, ActionInfo> actionInfoMap;
		public Func<string> LazyGroupID;

		public NotifyInfo(INotifyReceiver notifyReceiver)
		{
			this.notifyReceiver = notifyReceiver;
			this.actionInfoMap	= new Dictionary<Type, ActionInfo>();
			this.LazyGroupID	= () => notifyReceiver.LazyGroupID?.Invoke();
		}

		public bool CheckCondition(Type type, string groupID)
		{
			string lazyGroupID		= this.LazyGroupID?.Invoke();

			bool bGroupMatch		= groupID == "" || groupID == lazyGroupID;
			bool bTypeCorrespond	= actionInfoMap.ContainsKey(type);

			return bGroupMatch && bTypeCorrespond;
		}
	}

    public class NotifySystem : CreateSingleton<NotifySystem>
    {
		List<NotifyInfo> notifyInfoList;

		protected override void InitSingleton()
	    {
			notifyInfoList = new List<NotifyInfo>();
		}

		public void RegisterNotify<T>(INotifyReceiver notifyReceiver, Action<MessageReceiver> notifyAction)
		{
			RegisterNotify<T>(notifyReceiver, null, notifyAction);
		}

		public void RegisterNotify<T>(INotifyReceiver notifyReceiver, ReceiveOption option, Action<MessageReceiver> notifyAction)
		{
			Type type			= typeof(T);
			Type msgBaseType	= typeof(MessageBase);

			if (!msgBaseType.IsAssignableFrom(type))
			{
				Debug.LogError("Message沒有這個型別 !");
				return;
			}

			NotifyInfo notifyInfo = null;

			foreach (NotifyInfo currInfo in notifyInfoList)
			{
				if (currInfo.notifyReceiver == notifyReceiver)
				{
					notifyInfo = currInfo;
					break;
				}
			}

			if (notifyInfo == null)
			{
				notifyInfo = new NotifyInfo(notifyReceiver);
				notifyInfoList.Add(notifyInfo);
			}

			if(notifyInfo.actionInfoMap.ContainsKey(type))
			{
				Debug.LogError($"{notifyInfo.notifyReceiver} 重複註冊同一個message {type} 囉");
				return;
			}

			notifyInfo.actionInfoMap.Add(type, new ActionInfo()
			{
				notifyReceiver	= notifyInfo.notifyReceiver,
				receiveOption	= option,
				receiverAction	= notifyAction,
			});
		}


		public void UnregisterNotify(INotifyReceiver notifyReceiver)
		{
			int idx = -1;

			for (int i = 0; i < notifyInfoList.Count; ++i)
			{
				if (notifyInfoList[i].notifyReceiver == notifyReceiver)
				{
					idx = i;
					break;
				}
			}

			if(notifyInfoList.IsValidIndex<NotifyInfo>(idx))
			{
				notifyInfoList.RemoveAt(idx);
			}			
		}

		public void SendMsg(MessageSender msgSender, string groupID)
		{
			Type type					= msgSender.GetMsgType();
			Queue<ActionInfo> infoQueue = new Queue<ActionInfo>();

			foreach (NotifyInfo currInfo in notifyInfoList)
			{
				if(currInfo.CheckCondition(type, groupID))
				{
					ActionInfo actionInfo = currInfo.actionInfoMap[type];

					// 先將符合的action記錄起來，讓option處理
					if (actionInfo != null)
					{
						infoQueue.Enqueue(actionInfo);
					}					
				}
			}

			// 實際執行action的地方
			while (infoQueue.Count > 0)
			{
				ActionInfo actionInfo = infoQueue.Dequeue();

				// 判斷是否有相依性問題
				if(NeedToWait(actionInfo, infoQueue))
				{
					infoQueue.Enqueue(actionInfo);

					continue;
				}

				actionInfo.receiverAction?.Invoke(new MessageReceiver(msgSender));
			}
		}

		private bool NeedToWait(ActionInfo actionInfo, Queue<ActionInfo> infoQueue)
		{
			if(actionInfo.receiveOption == null)
			{
				// 沒有option 就不用設定Wait
				return false;
			}

			bool bResult		= false;
			List<Type> typeList = actionInfo.receiveOption.dependOnList;

			foreach (ActionInfo info in infoQueue)
			{
				INotifyReceiver notifyReceiver = info.notifyReceiver;

				if (typeList.Contains(notifyReceiver.GetType()))
				{
					return true;
				}
			}

			return bResult;
		}
	}
}
