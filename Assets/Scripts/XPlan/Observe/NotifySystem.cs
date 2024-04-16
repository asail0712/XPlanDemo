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

    public class NotifySystem : CreateSingleton<NotifySystem>
    {
		Dictionary<Type, List<INotifyReceiver>> ReceiveMap;

        protected override void InitSingleton()
	    {
			ReceiveMap = new Dictionary<Type, List<INotifyReceiver>>();
		}

		public void RegisterNotify(Type type, INotifyReceiver receiver)
		{
			Type msgBaseType = typeof(MessageBase);

			if(!msgBaseType.IsAssignableFrom(type))
			{
				Debug.LogError("Message沒有這個型別 !");
			}

			List<INotifyReceiver> receiveList = ReceiveMap.FindOrAdd<Type, List<INotifyReceiver>>(type);

			receiveList.Add(receiver);
		}

		public void UnregisterNotify(Type type, INotifyReceiver receiver)
		{
			Type msgBaseType = typeof(MessageBase);

			if (!msgBaseType.IsAssignableFrom(type))
			{
				Debug.LogError("Message沒有這個型別 !");
			}

			if (!ReceiveMap.ContainsKey(type))
			{
				return;
			}

			List<INotifyReceiver> receiveList = ReceiveMap[type];

			receiveList.Remove(receiver);
		}

		public void SendMsg(MessageSender msgSender)
		{
			Type type = msgSender.GetType();

			if(!ReceiveMap.ContainsKey(type))
			{
				//Debug.Log($"No Using Message => {type}!!");
				return;
			}

			List<INotifyReceiver> receiveList = ReceiveMap[type];

			receiveList.ForEach((E04) => 
			{
				E04.ReceiveNotify(new MessageReceiver(msgSender));
			});
		}
	}
}
