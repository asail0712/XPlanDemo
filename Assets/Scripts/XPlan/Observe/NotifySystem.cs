using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XPlan.Utility;
using XPlan.Utility.Extensions;

namespace XPlan.Observe
{
	public class MessageBase
	{		
		protected void Send()
		{
			MessageSender sender = new MessageSender(this);

			sender.SendMessage();
		}
	}

	public class MessageReceiver
	{
		private MessageBase msg;

		public MessageReceiver(MessageBase msg)
		{
			this.msg = msg;
		}

		public bool CorrespondType(Type type)
		{
			return msg.GetType() == type;
		}

		public T GetMessage<T>() where T : MessageBase
		{
			return (T)msg;
		}
	}


	public class MessageSender
	{
		private MessageBase msg;

		public MessageSender(MessageBase msg)
		{
			this.msg	= msg;
		}

		public void SendMessage()
		{
			NotifySystem.Instance.SendMsg(msg);
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

		public void SendMsg(MessageBase msg)
		{
			Type type = msg.GetType();

			if(!ReceiveMap.ContainsKey(type))
			{
				//Debug.Log($"No Using Message => {type}!!");
				return;
			}

			List<INotifyReceiver> receiveList = ReceiveMap[type];

			receiveList.ForEach((E04) => 
			{
				E04.ReceiveNotify(new MessageReceiver(msg));
			});
		}
	}
}
