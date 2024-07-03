using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

using XPlan.Extensions;
using XPlan.Interface;

namespace XPlan.UI
{
	public class UIParam
	{
		private object param;

		public T GetValue<T>()
		{
			return (T)param;
		}

		public UIParam(object p)
		{
			param = p;
		}
	}

	public class ActionInfo
	{
		public UIBase ui;
		public ListenOption listenOption;
		public Action<UIParam[]> callingAction;
	}

	struct CallbackGroup
	{
		public IUIListener uiListener;
		public Action<UIParam> callback;

		public CallbackGroup(IUIListener u, Action<UIParam> c)
		{
			uiListener	= u;
			callback	= c;
		}
	}

	public static class UISystem
	{
		static private List<Func<bool>> pauseList = new List<Func<bool>>();

		/**********************************************
		* Call Back相關功能
		* ********************************************/

		static private Dictionary<string, List<CallbackGroup>> callbackDict = new Dictionary<string, List<CallbackGroup>>();

		static public void RegisterCallback(string uniqueID, IUIListener handler, Action<UIParam> callback)
		{			
			if (!callbackDict.ContainsKey(uniqueID))
			{
				callbackDict[uniqueID] = new List<CallbackGroup>();
			}

			callbackDict[uniqueID].Add(new CallbackGroup(handler, callback));
		}

		static public void UnregisterCallback(string uniqueID, IUIListener listener)
		{
			if (callbackDict.ContainsKey(uniqueID))
			{
				callbackDict[uniqueID].RemoveAll((group) => 
				{
					return group.uiListener == listener;
				});
			}
		}

		static public void UnregisterAllCallback(IUIListener listener)
		{
			foreach (KeyValuePair<string, List<CallbackGroup>> kvp in callbackDict)
			{
				List<CallbackGroup> groupList = kvp.Value;

				groupList.RemoveAll((group)=> 
				{
					return group.uiListener == listener;
				});
			}
		}

		static public bool HasKey(string uniqueID)
		{
			return callbackDict.ContainsKey(uniqueID);
		}

		static public void TriggerCallback(string uniqueID, Action onPress)
		{
			if (CheckToPause())
			{
				return;
			}	

			// onPress主要是讓 UI呼叫的，所以不管Handler是否有註冊都要執行
			onPress?.Invoke();

			if (!callbackDict.ContainsKey(uniqueID))
			{
				return;
			}

			List<CallbackGroup> groupList = callbackDict[uniqueID];

			groupList.ForEach((group) =>
			{
				// onPostPress 是當而完成click要做的
				group.callback?.Invoke(null);
			});
		}

		static public void TriggerCallback<T>(string uniqueID, T param, Action<T> onPress)
		{
			// 阻擋任何UI操作，主要用於手機App當網路還沒回應完成的時候
			if (CheckToPause())
			{
				return;
			}

			UIParam uiParam = new UIParam(param);

			// onPress主要是讓 UI呼叫的，所以不管Handler是否有註冊都要執行
			onPress?.Invoke(uiParam.GetValue<T>());

			if (!callbackDict.ContainsKey(uniqueID))
			{
				return;
			}

			List<CallbackGroup> groupList = callbackDict[uniqueID];

			groupList.ForEach((group) =>
			{
				// onPostPress 是當而完成click要做的
				group.callback?.Invoke(uiParam);
			});
		}

		/**********************************************
		* 暫停功能
		* ********************************************/
		static public bool CheckToPause()
		{
			bool bNeedToPause = false;

			foreach (Func<bool> checkPause in pauseList)
			{
				if(checkPause())
				{
					bNeedToPause = true;
					break;
				}
			}

			return bNeedToPause;
		}

		static public void AddPauseFunc(Func<bool> func)
		{
			pauseList.Add(func);
		}

		static public void RemovePauseFunc(Func<bool> func)
		{
			pauseList.Remove(func);
		}

		/**********************************************
		* Sync Calling相關功能
		* ********************************************/
		public class CallingInfo
		{
			public UIBase ui;
			public Dictionary<string, ActionInfo> callingMap;

			public CallingInfo(UIBase ui)
			{
				this.ui			= ui;
				this.callingMap = new Dictionary<string, ActionInfo>();
			}
		}

		static private List<CallingInfo> callingList = new List<CallingInfo>();

		static public void ListenCall(string id, UIBase ui, ListenOption option, Action<UIParam[]> callingAction)
		{
			// 尋找對應的UICallingInfo

			int idx = callingList.FindIndex((E04) =>
			{
				return E04.ui == ui;
			});

			CallingInfo callingInfo = null;

			if (!callingList.IsValidIndex<CallingInfo>(idx))
			{
				callingInfo = new CallingInfo(ui);

				callingList.Add(callingInfo);
			}
			else
			{
				callingInfo = callingList[idx];
			}

			// 將資料放進 UICallingInfo
			if (callingInfo.callingMap.ContainsKey(id))
			{
				Debug.LogError($"{ui} 重複註冊同一個 UICommand {id} 囉");
				return;
			}

			callingInfo.callingMap.Add(id, new ActionInfo() 
			{
				ui				= ui,
				listenOption	= option,
				callingAction	= callingAction
			});
		}

		static public void UnlistenAllCall(UIBase ui)
		{
			callingList.RemoveAll((E04) => 
			{
				return E04.ui == ui;
			});
		}

		static public void DirectCall<T>(string uniqueID, T value)
		{			
			// 參數轉成陣列
			List<UIParam> paramList = new List<UIParam>();
			paramList.Add(new UIParam(value));

			DirectCall_Internal(uniqueID, paramList.ToArray());
		}

		static public void DirectCall(string uniqueID)
		{
			DirectCall_Internal(uniqueID, new List<UIParam>().ToArray());
		}

		static public void DirectCall(string uniqueID, params object[] paramArr)
		{
			List<UIParam> paramList = new List<UIParam>();

			for (int i = 0; i < paramArr.Length; ++i)
			{
				paramList.Add(new UIParam(paramArr[i]));
			}

			DirectCall_Internal(uniqueID, paramList.ToArray());
		}

		static private void DirectCall_Internal(string uniqueID, UIParam[] paramArr)
		{
			Queue<ActionInfo> infoQueue = new Queue<ActionInfo>();

			foreach (CallingInfo info in callingList)
			{
				foreach (KeyValuePair<string, ActionInfo> kvp in info.callingMap)
				{
					if (kvp.Key == uniqueID)
					{
						infoQueue.Enqueue(kvp.Value);
					}
				}
			}

			List<UIParam> paramList = new List<UIParam>();

			for (int i = 0; i < paramArr.Length; ++i)
			{
				paramList.Add(new UIParam(paramArr[i]));
			}

			// 實際執行action的地方
			while (infoQueue.Count > 0)
			{
				ActionInfo actionInfo = infoQueue.Dequeue();

				// 判斷是否有設定相依，有的話，慢點去執行
				if (NeedToWait(actionInfo, infoQueue))
				{
					infoQueue.Enqueue(actionInfo);

					continue;
				}

				actionInfo.callingAction?.Invoke(paramArr);
			}
		}

		static private bool NeedToWait(ActionInfo actionInfo, Queue<ActionInfo> infoQueue)
		{
			if (actionInfo.listenOption == null)
			{
				// 沒有option 就不用設定Wait
				return false;
			}

			bool bResult = false;
			List<Type> typeList = actionInfo.listenOption.dependOnList;

			foreach (ActionInfo info in infoQueue)
			{
				UIBase notifyReceiver = info.ui;

				if (typeList.Contains(notifyReceiver.GetType()))
				{
					return true;
				}
			}

			return bResult;
		}
	}
}

