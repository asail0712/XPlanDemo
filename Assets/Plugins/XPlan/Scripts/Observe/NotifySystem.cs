using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XPlan.DebugMode;
using XPlan.Utility;

namespace XPlan.Observe
{
	public class MessageBase
	{
		public void Send(string zoneID = "", bool bAsync = false)
		{
			MessageSender sender = new MessageSender(this);

            if(bAsync)
            {
                sender.SendMessageAsync(zoneID);
            }
            else
            {
                sender.SendMessage(zoneID);
            }                
		}
	}

	public class ReceiveOption
	{
		// 相依性
		public List<Type> dependOnList;
	}

	public class ReceiverInfo
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

		public T GetMessage<T>(bool bShowLog = false) where T : MessageBase
		{
			if(bShowLog)
			{ 
#if DEBUG
				string className	= msgSender.stackInfo.GetClassName();
				string methodName	= msgSender.stackInfo.GetMethodName();
				string lineNumber	= msgSender.stackInfo.GetLineNumber();
				string fullLogInfo	= $"Notify({msgSender.msg.GetType()}) from [ {className}::{methodName}() ], line {lineNumber} ";

				LogSystem.Record(fullLogInfo);
#endif //DEBUG
			}

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

		public void SendMessage(string zoneID)
		{
			NotifySystem.Instance.SendMsg(this, zoneID);
		}

        public void SendMessageAsync(string zoneID)
        {
            NotifySystem.Instance.SendMsgAsync(this, zoneID);
        }

        public Type GetMsgType()
		{
			return msg.GetType();
		}
	}

	public class NotifyInfo
	{
		public INotifyReceiver notifyReceiver;
		public Dictionary<Type, ReceiverInfo> receiveInfoMap;
		public Func<string> LazyZoneID;

		public NotifyInfo(INotifyReceiver notifyReceiver)
		{
			this.notifyReceiver = notifyReceiver;
			this.receiveInfoMap = new Dictionary<Type, ReceiverInfo>();
			this.LazyZoneID	= () => notifyReceiver.GetLazyZoneID?.Invoke();
		}

		public bool CheckCondition(Type type, string zoneID)
		{
			string lazyZoneID		= this.LazyZoneID?.Invoke();

			bool bZoneMatch			= string.IsNullOrEmpty(zoneID) || zoneID == lazyZoneID;
			bool bTypeCorrespond	= receiveInfoMap.ContainsKey(type);

			return bZoneMatch && bTypeCorrespond;
		}
	}

    public class NotifySystem : CreateSingleton<NotifySystem>
    {
		private List<NotifyInfo> notifyInfoList;

        private Queue<(MessageSender, string)> senderQueue;

		protected override void InitSingleton()
	    {
			notifyInfoList  = new List<NotifyInfo>();
            senderQueue     = new Queue<(MessageSender, string)>();

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

			if(notifyInfo.receiveInfoMap.ContainsKey(type))
			{
				Debug.LogWarning($"{notifyInfo.notifyReceiver} 重複註冊同一個message {type} 囉");
				notifyInfo.receiveInfoMap.Remove(type);

            }

			notifyInfo.receiveInfoMap.Add(type, new ReceiverInfo()
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

		public void SendMsg(MessageSender msgSender, string zoneID)
		{
			Type type					    = msgSender.GetMsgType();
			Queue<ReceiverInfo> infoQueue   = new Queue<ReceiverInfo>();

			foreach (NotifyInfo currInfo in notifyInfoList)
			{
				if(currInfo.CheckCondition(type, zoneID))
				{
                    ReceiverInfo receiveInfo = currInfo.receiveInfoMap[type];

					// 先將符合的action記錄起來，讓option處理
					if (receiveInfo != null)
					{
						infoQueue.Enqueue(receiveInfo);
					}					
				}
			}

			// 實際執行action的地方
			while (infoQueue.Count > 0)
			{
                ReceiverInfo receiveInfo = infoQueue.Dequeue();

				// 判斷是否有相依性問題
				if(NeedToWait(receiveInfo, infoQueue))
				{
					infoQueue.Enqueue(receiveInfo);

					continue;
				}

                receiveInfo.receiverAction?.Invoke(new MessageReceiver(msgSender));
			}
		}

        public void SendMsgAsync(MessageSender msgSender, string zoneID)
        {
            senderQueue.Enqueue((msgSender, zoneID));
        }

        public void Update()
        {
            while(senderQueue.Count > 0)
            {
                var senderInfo          = senderQueue.Dequeue();
                MessageSender msgSender = senderInfo.Item1;
                string zoneID           = senderInfo.Item2;

                SendMsg(msgSender, zoneID);
            }
        }

        private bool NeedToWait(ReceiverInfo receiveInfo, Queue<ReceiverInfo> infoQueue)
		{
			if(receiveInfo.receiveOption == null)
			{
				// 沒有option 就不用設定Wait
				return false;
			}

			bool bResult		= false;
			List<Type> typeList = receiveInfo.receiveOption.dependOnList;

			foreach (ReceiverInfo info in infoQueue)
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
