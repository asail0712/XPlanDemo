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
		public void Send()
		{
			MessageSender sender = new MessageSender(this);

			sender.SendMessage();
		}
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
			Debug.Log(fullLogInfo);
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

		public void SendMessage()
		{
			NotifySystem.Instance.SendMsg(this);
		}

		public new Type GetType()
		{
			return msg.GetType();
		}
	}

	public class NotifyInfo
	{
		public INotifyReceiver notifyReceiver;
		public Dictionary<Type, List<Action<MessageReceiver>>> typeReceiveMap;

		public NotifyInfo(INotifyReceiver notifyReceiver)
		{
			this.notifyReceiver = notifyReceiver;
			this.typeReceiveMap = new Dictionary<Type, List<Action<MessageReceiver>>>();
		}
	}


    public class NotifySystem : CreateSingleton<NotifySystem>
    {
		List<NotifyInfo> infoList;

		protected override void InitSingleton()
	    {
			infoList = new List<NotifyInfo>();
		}

		public void RegisterNotify<T>(INotifyReceiver notifyReceiver, Action<MessageReceiver> notifyAction)
		{
			Type type			= typeof(T);
			Type msgBaseType	= typeof(MessageBase);

			if (!msgBaseType.IsAssignableFrom(type))
			{
				Debug.LogError("Message沒有這個型別 !");
				return;
			}

			NotifyInfo notifyInfo = null;

			foreach (NotifyInfo currInfo in infoList)
			{
				if(currInfo.notifyReceiver == notifyReceiver)
				{
					notifyInfo = currInfo;
					break;
				}
			}

			if(notifyInfo == null)
			{
				notifyInfo = new NotifyInfo(notifyReceiver);
				infoList.Add(notifyInfo);
			}

			List<Action<MessageReceiver>> actionList = notifyInfo.typeReceiveMap.FindOrAdd<Type, List<Action<MessageReceiver>>>(type);
			actionList.Add(notifyAction);
		}

		public void UnregisterNotify(INotifyReceiver notifyReceiver)
		{
			int idx = -1;

			for (int i = 0; i < infoList.Count; ++i)
			{
				if (infoList[i].notifyReceiver == notifyReceiver)
				{
					idx = i;
					break;
				}
			}

			if(infoList.IsValidIndex<NotifyInfo>(idx))
			{
				infoList.RemoveAt(idx);
			}			
		}

		public void SendMsg(MessageSender msgSender)
		{
			Type type = msgSender.GetType();

			foreach (NotifyInfo currInfo in infoList)
			{
				if(currInfo.typeReceiveMap.ContainsKey(type))
				{
					List<Action<MessageReceiver>> actionList = currInfo.typeReceiveMap[type];

					foreach (Action<MessageReceiver> action in actionList)
					{
						action?.Invoke(new MessageReceiver(msgSender));
					}
				}
			}
		}
	}
}
