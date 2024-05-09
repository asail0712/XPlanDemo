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
		public static List<Func<bool>> pauseList = new List<Func<bool>>();

		private static readonly string checkStr = "No Check";

		static private void CheckLog(string uniqueID, string logContent)
		{
			if(string.IsNullOrEmpty(checkStr) || uniqueID == checkStr)
			{
				Debug.Log($"{logContent} {uniqueID}");
			}			
		}

		/**********************************************
		* 通用功能
		* ********************************************/
		static public UIParam GetUIParam(this object param)
		{
			return new UIParam(param);
		}

		/**********************************************
		* Call Back相關功能
		* ********************************************/

		static private Dictionary<string, List<CallbackGroup>> callbackDict = new Dictionary<string, List<CallbackGroup>>();

		static public void RegisterCallback(string uniqueID, IUIListener handler, Action<UIParam> callback)
		{
			CheckLog(uniqueID, "加掛UI監聽");

			if (!callbackDict.ContainsKey(uniqueID))
			{
				callbackDict[uniqueID] = new List<CallbackGroup>();
			}

			callbackDict[uniqueID].Add(new CallbackGroup(handler, callback));
		}

		static public void UnregisterCallback(string uniqueID, IUIListener l)
		{
			if (callbackDict.ContainsKey(uniqueID))
			{
				callbackDict[uniqueID].RemoveAll((group) => 
				{
					return group.uiListener == l;
				});
			}
		}

		static public void UnregisterAllCallback(IUIListener l)
		{
			foreach (KeyValuePair<string, List<CallbackGroup>> kvp in callbackDict)
			{
				List<CallbackGroup> groupList = kvp.Value;

				groupList.RemoveAll((group)=> 
				{
					return group.uiListener == l;
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
				CheckLog(uniqueID, "執行UI監聽");

				// onPostPress 是當而完成click要做的
				group.callback?.Invoke(null);
			});
		}

		static public void TriggerCallback<T>(string uniqueID, T param, Action<T> onPress)
		{
			// 阻擋任何UI操作，用於手機App當網路還沒回應完成的時候
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
				CheckLog(uniqueID, "執行UI監聽");

				// onPostPress 是當而完成click要做的
				group.callback?.Invoke(uiParam);
			});
		}

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

		/**********************************************
		* Sync Calling相關功能
		* ********************************************/
		public class UICallingInfo
		{
			public UIBase ui;
			public Dictionary<string, List<Action<UIParam[]>>> callingeMap;

			public UICallingInfo(UIBase ui)
			{
				this.ui				= ui;
				this.callingeMap	= new Dictionary<string, List<Action<UIParam[]>>>();
			}
		}
		static private List<UICallingInfo> callingList = new List<UICallingInfo>();

		static public void ListenCall(string id, UIBase ui, Action<UIParam[]> callingAction)
		{
			// 尋找對應的UICallingInfo

			int idx = callingList.FindIndex((E04) =>
			{
				return E04.ui == ui;
			});

			UICallingInfo callingInfo = null;

			if (!callingList.IsValidIndex<UICallingInfo>(idx))
			{
				callingInfo = new UICallingInfo(ui);

				callingList.Add(callingInfo);
			}
			else
			{
				callingInfo = callingList[idx];
			}

			// 將資料放進 UICallingInfo

			if (!callingInfo.callingeMap.ContainsKey(id))
			{
				List<Action<UIParam[]>> actionList = new List<Action<UIParam[]>>();
				actionList.Add(callingAction);

				callingInfo.callingeMap.Add(id, actionList);
			}
			else
			{
				callingInfo.callingeMap[id].Add(callingAction);
			}	
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
			List<Action<UIParam[]>> totalActionList = new List<Action<UIParam[]>>();

			foreach (UICallingInfo info in callingList)
			{
				foreach (KeyValuePair<string, List<Action<UIParam[]>>> kvp in info.callingeMap)
				{
					if(kvp.Key == uniqueID)
					{
						totalActionList.AddRange(kvp.Value);
					}
				}
			}

			// 參數轉成陣列
			List<UIParam> paramList = new List<UIParam>();
			paramList.Add(new UIParam(value));

			foreach (Action<UIParam[]> action in totalActionList)
			{
				action?.Invoke(paramList.ToArray());
			}
		}

		static public void DirectCall(string uniqueID)
		{
			List<Action<UIParam[]>> totalActionList = new List<Action<UIParam[]>>();

			foreach (UICallingInfo info in callingList)
			{
				foreach (KeyValuePair<string, List<Action<UIParam[]>>> kvp in info.callingeMap)
				{
					if (kvp.Key == uniqueID)
					{
						totalActionList.AddRange(kvp.Value);
					}
				}
			}

			foreach (Action<UIParam[]> action in totalActionList)
			{
				action?.Invoke(new List<UIParam>().ToArray());
			}
		}

		static public void DirectCall(string uniqueID, params object[] paramArr)
		{
			List<Action<UIParam[]>> totalActionList = new List<Action<UIParam[]>>();

			foreach (UICallingInfo info in callingList)
			{
				foreach (KeyValuePair<string, List<Action<UIParam[]>>> kvp in info.callingeMap)
				{
					if (kvp.Key == uniqueID)
					{
						totalActionList.AddRange(kvp.Value);
					}
				}
			}

			List<UIParam> paramList = new List<UIParam>();

			for (int i = 0; i < paramArr.Length; ++i)
			{
				paramList.Add(new UIParam(paramArr[i]));
			}

			foreach (Action<UIParam[]> action in totalActionList)
			{
				action?.Invoke(paramList.ToArray());
			}
		}
	}
}

