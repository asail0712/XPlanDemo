using TMPro;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using XPlan.Interface;
using XPlan.Scenes;
using XPlan.UI.Component;

namespace XPlan.UI
{
	public class ListenOption
	{
		// 相依性
		public List<Type> dependOnList = new List<Type>();

		public void AddDepondOn(Type type)
		{
			dependOnList.Add(type);
		}
	}

	public class UIBase : MonoBehaviour, IUIListener
	{
		// 判斷是否由UILoader仔入
		private bool bSpawnByLoader = false;

		/********************************
		* Listen Handler Call
		* *****************************/
		public void ListenCall(string id, ListenOption option, Action<UIParam[]> paramAction)
		{
			UISystem.ListenCall(id, this, option, (paramList) =>
			{
				if (bSpawnByLoader &&
					UIController.IsInstance()
					&& !UIController.Instance.IsWorkingUI(this))
				{
					return;
				}

				paramAction?.Invoke(paramList);
			});
		}

		public void ListenCall<T>(string id, ListenOption option, Action<T> paramAction)
		{
			UISystem.ListenCall(id, this, option, (paramList) =>
			{
				if (bSpawnByLoader &&
					UIController.IsInstance()
					&& !UIController.Instance.IsWorkingUI(this))
				{
					return;
				}

				paramAction?.Invoke(paramList[0].GetValue<T>());
			});
		}

		public void ListenCall(string id, ListenOption option, Action noParamAction)
		{
			UISystem.ListenCall(id, this, option, (paramList) =>
			{
				if (bSpawnByLoader &&
					UIController.IsInstance()
					&& !UIController.Instance.IsWorkingUI(this))
				{
					return;
				}

				noParamAction?.Invoke();
			});
		}

		public void ListenCall(string id, Action<UIParam[]> paramsAction)
		{
			ListenCall(id, null, paramsAction);
		}

		public void ListenCall<T>(string id, Action<T> paramAction)
		{
			ListenCall<T>(id, null, paramAction);
		}

		public void ListenCall(string id, Action noParamAction)
		{
			ListenCall(id, null, noParamAction);
		}

		/********************************
		 * 註冊UI callback
		 * *****************************/
		protected void RegisterButton<T>(string uniqueID, Button button, Func<T> onLazyGet, Action<T> onPress = null)
		{
			button.onClick.AddListener(() =>
			{
				DirectTrigger<T>(uniqueID, onLazyGet.Invoke(), onPress);
			});
		}

		protected void RegisterButton<T>(string uniqueID, Button button, T param = default(T), Action<T> onPress = null)
		{
			button.onClick.AddListener(() =>
			{
				DirectTrigger<T>(uniqueID, param, onPress);
			});
		}

		protected void RegisterButton(string uniqueID, Button button, Action onPress = null)
		{
			button.onClick.AddListener(() =>
			{
				DirectTrigger(uniqueID, onPress);
			});
		}
		protected void RegisterText(string uniqueID, TMP_InputField inputTxt, Action<string> onPress = null)
		{
			inputTxt.onValueChanged.AddListener((str) =>
			{
				DirectTrigger<string>(uniqueID, str, onPress);
			});
		}

		protected void RegisterText(string uniqueID, InputField inputTxt, Action<string> onPress = null)
		{
			inputTxt.onValueChanged.AddListener((str) =>
			{
				DirectTrigger<string>(uniqueID, str, onPress);
			});
		}

		protected void RegisterSlider(string uniqueID, Slider slider, Action<float> onPress = null)
		{
			slider.onValueChanged.AddListener((value) =>
			{
				DirectTrigger<float>(uniqueID, value, onPress);
			});
		}

		protected void RegisterDropdown(string uniqueID, TMP_Dropdown dropdown, Action<string> onPress = null)
		{
			dropdown.onValueChanged.AddListener((idx) =>
			{
				string str = dropdown.options[idx].text;

				DirectTrigger<string>(uniqueID, str, onPress);
			});
		}

		protected void RegisterDropdown(string uniqueID, Dropdown dropdown, Action<string> onPress = null)
		{
			dropdown.onValueChanged.AddListener((idx) =>
			{
				string str = dropdown.options[idx].text;

				DirectTrigger<string>(uniqueID, str, onPress);
			});
		}

		protected void RegisterToggle(string uniqueID, Toggle toggle, Action<bool> onPress = null)
		{
			toggle.onValueChanged.AddListener((bOn) =>
			{
				UISystem.TriggerCallback<bool>(uniqueID, bOn, onPress);
			});
		}

		protected void RegisterToggles(string uniqueID, Toggle[] toggleArr, bool bCancelSelf = false, Action<int> onPress = null)
		{
			foreach (Toggle toggle in toggleArr)
			{
				if (toggle == null)
				{
					continue;
				}

				toggle.onValueChanged.AddListener((bOn) =>
				{
					// 只要收取按下的那個label即可
					if (!bOn)
					{
						// 重複點擊可以自我取消
						if (bCancelSelf)
						{
							toggle.SetIsOnWithoutNotify(false);
						}
						else
						{
							return;
						}
					}

					int idx = Array.IndexOf(toggleArr, toggle);

					UISystem.TriggerCallback<int>(uniqueID, idx, onPress);
				});
			}
		}

		protected void RegisterToggleBtns(string uniqueID, Button[] buttonArr, int defaultIndex = 0, Action<int> onPress = null)
		{
			// 初始化
			for(int i = 0; i < buttonArr.Length; ++i)
			{
				Button btn = buttonArr[i];

				btn.gameObject.SetActive(i == defaultIndex);

				btn.onClick.AddListener(() =>
				{
					int currIdx		= Array.IndexOf(buttonArr, btn);
					int chooseIdx	= (currIdx + 1) % buttonArr.Length;

					for (int i = 0; i < buttonArr.Length; ++i)
					{
						buttonArr[i].gameObject.SetActive(i == chooseIdx);
					}

					DirectTrigger<int>(uniqueID, chooseIdx, onPress);
				});
			}
		}

		protected void RegisterPointTrigger(string uniqueID, PointEventTriggerHandler pointTrigger,
												Action<PointerEventData> onPress = null,
												Action<PointerEventData> onPull = null)
		{
			pointTrigger.OnPointDown += (val) =>
			{
				onPress?.Invoke(val);

				UISystem.TriggerCallback<bool>(uniqueID, true, null);
			};

			pointTrigger.OnPointUp += (val) =>
			{
				onPull?.Invoke(val);

				UISystem.TriggerCallback<bool>(uniqueID, false, null);
			};
		}

		protected void DirectTrigger<T>(string uniqueID, T param, Action<T> onPress = null)
		{
			UISystem.TriggerCallback<T>(uniqueID, param, onPress);
		}

		protected void DirectTrigger(string uniqueID, Action onPress = null)
		{
			UISystem.TriggerCallback(uniqueID, onPress);
		}

		/********************************
		* UI間的溝通
		* *****************************/
		protected void AddUIListener<T>(string uniqueID, Action<T> callback)
		{
			UISystem.RegisterCallback(uniqueID, this, (param) =>
			{
				callback?.Invoke(param.GetValue<T>());
			});
		}

		protected void AddUIListener(string uniqueID, Action callback)
		{
			UISystem.RegisterCallback(uniqueID, this, (dump) =>
			{
				callback?.Invoke();
			});
		}

		/********************************
		 * 流程
		 * *****************************/
		protected Action<float> onTickEvent;

		private void Update()
		{
			onTickEvent?.Invoke(Time.deltaTime);
		}

		private void OnDestroy()
		{
			OnDispose();

			UISystem.UnlistenAllCall(this);
			UISystem.UnregisterAllCallback(this);
		}

		protected virtual void OnDispose()
		{
			// for override
		}

		/********************************
		 * 初始化
		 * *****************************/

		private int sortIdx = -1;

		protected virtual void OnInitialUI()
		{
			// for overrdie
		}

		public void InitialUI(int idx)
		{
			this.sortIdx		= idx;
			this.bSpawnByLoader = true;

			OnInitialUI();
		}
		public int SortIdx { get => sortIdx; set => sortIdx = value; }
		
		/********************************
		 * 其他
		 * *****************************/
		public string GetStr(string keyStr, bool bShowWarning = false)
		{
			return UIController.Instance.GetStr(keyStr, bShowWarning);
		}
	}
}

