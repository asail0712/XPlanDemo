using System;
using System.Collections.Generic;
using UnityEngine;

using XPlan.Interface;

namespace XPlan.UI
{
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
			UIParam p = null;

			if (param is int)
			{
				p = new IntParam((int)param);
			}
			else if (param is string)
			{
				p = new StringParam((string)param);
			}
			else if (param is float)
			{
				p = new FloatParam((float)param);
			}
			else if (param is double)
			{
				p = new DoubleParam((double)param);
			}
			else if (param is bool)
			{
				p = new BoolParam((bool)param);
			}
			else if (param is Vector2)
			{
				p = new Vector2Param((Vector2)param);
			}
			else if (param is byte[])
			{
				p = new ByteArrParam((byte[])param);
			}
			else if (param is Texture)
			{
				p = new TextureParam((Texture)param);
			}
			else if (param is Action)
			{
				p = new ActionParam((Action)param);
			}
			else if (param is UIDataContainer)
			{
				p = (UIDataContainer)param;
			}

			return p;
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

		static public void TriggerCallback<T>(string uniqueID, UIParam param, Action<T> onPress)
		{
			if (CheckToPause())
			{
				return;
			}

			// onPress主要是讓 UI呼叫的，所以不管Handler是否有註冊都要執行
			onPress?.Invoke(param.GetValue<T>());

			if (!callbackDict.ContainsKey(uniqueID))
			{
				return;
			}

			List<CallbackGroup> groupList = callbackDict[uniqueID];

			groupList.ForEach((group) =>
			{
				CheckLog(uniqueID, "執行UI監聽");

				// onPostPress 是當而完成click要做的
				group.callback?.Invoke(param);
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
		static private Dictionary<UIBase, List<string>> uiCallingDict = new Dictionary<UIBase, List<string>>();

		static public void ListenCall(string id, UIBase ui)
		{
			if(!uiCallingDict.ContainsKey(ui))
			{
				uiCallingDict.Add(ui, new List<string>());
			}

			List<string> idList = uiCallingDict[ui];

			if(!idList.Contains(id))
			{
				// 避免重複呼叫
				uiCallingDict[ui].Add(id);
			}			
		}

		static public void UnlistenCall(string id, UIBase ui)
		{
			if (!uiCallingDict.ContainsKey(ui))
			{
				return;
			}

			uiCallingDict[ui].Remove(id);
		}

		static public void UnlistenAllCall(UIBase ui)
		{
			uiCallingDict.Remove(ui);
		}

		struct NotifyUIInfo
		{
			public UIBase ui;
			public string uniqueID;
			public UIParam param;

			public NotifyUIInfo(UIBase u, string s, UIParam p)
			{
				ui			= u;
				uniqueID	= s;
				param		= p;
			}
		};

		static public void DirectCall<T>(string uniqueID, T value)
		{
			List<NotifyUIInfo> notifyList = new List<NotifyUIInfo>();

			foreach (KeyValuePair<UIBase, List<string>> kvp in uiCallingDict)
			{
				if(kvp.Value.Contains(uniqueID))
				{
					// 個別UI個別轉型
					UIParam p = value.GetUIParam();

					if (p == null)
					{
						Debug.LogError("UISystem not support this type !!");
						return;
					}

					// 因為uiCallingDict為成員變數
					// Notify出去後，有可能會修改到uiCallingDict
					// 所以要拆成兩個動作處理
					//kvp.Key.NotifyUI(uniqueID, p);
					NotifyUIInfo info = new NotifyUIInfo(kvp.Key, uniqueID, p);

					notifyList.Add(info);
				}
			}

			foreach (NotifyUIInfo info in notifyList)
			{
				info.ui.NotifyUI(info.uniqueID, info.param);
			}
		}

		static public void DirectCall(string uniqueID)
		{
			foreach (KeyValuePair<UIBase, List<string>> kvp in uiCallingDict)
			{
				if (kvp.Value.Contains(uniqueID))
				{					
					kvp.Key.NotifyUI(uniqueID, new UIParam());
				}
			}
		}

		static public void DirectCall(string uniqueID, params object[] paramList)
		{
			foreach (KeyValuePair<UIBase, List<string>> kvp in uiCallingDict)
			{
				if (kvp.Value.Contains(uniqueID))
				{
					int len			= paramList.Length;
					UIParam[] pList = new UIParam[len];

					for(int i = 0; i < len; ++i)
					{
						pList[i] = paramList[i].GetUIParam();
					}

					kvp.Key.NotifyUI(uniqueID, pList);
				}
			}
		}
	}
}

