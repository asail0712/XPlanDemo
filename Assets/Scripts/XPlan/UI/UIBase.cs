using TMPro;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using XPlan.Interface;
using XPlan.Scenes;

namespace XPlan.UI
{
	public class UIBase : MonoBehaviour, IUIListener
	{
		// 判斷是否由UILoader仔入
		public bool bSpawnByLoader = false;

		/********************************
		* Listen Handler Call
		* *****************************/
		public void ListenCall(string id)
		{
			UISystem.ListenCall(id, this);
		}

		public void NotifyUI(string uniqueID, params UIParam[] value)
		{
			// 並非由UILoader生成的UI不檢查是否working
			if (bSpawnByLoader && 
				UIController.IsInstance()
				&& !UIController.Instance.IsWorkingUI(this))
			{
				return;
			}

			OnNotifyUI(uniqueID, value);
		}

		protected virtual void OnNotifyUI(string uniqueID, params UIParam[] value)
		{
			// for override
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

		protected void RegisterToggle(string uniqueID, Toggle toggle, Action<bool> onPress = null)
		{
			toggle.onValueChanged.AddListener((bOn)=> 
			{
				UISystem.TriggerCallback<bool>(uniqueID, bOn, onPress);				
			});
		}

		protected void RegisterLabels(string uniqueID, UILabel[] labelArr, Action<int> onPress = null)
		{
			Array.ForEach(labelArr, (X) => 
			{
				X.onValueChanged.AddListener((bOn) =>
				{
					// 只要收取按下的那個label即可
					if(!bOn)
					{
						return;
					}

					UISystem.TriggerCallback<int>(uniqueID, X.labelIdx, onPress);
				});
			});
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

		bool bNoNewAnyThing = false;

		protected Action<float> onTickEvent;

		private void Update()
		{
			onTickEvent?.Invoke(Time.deltaTime);
		}

		private void OnDestroy()
		{
			OnDispose();

			if(!bNoNewAnyThing)
			{
				SceneController.Instance.UnregisterFadeCallback(sceneType, TriggerToFadeOut, IsFadeOutFinish);
			}
			
			UISystem.UnlistenAllCall(this);
			UISystem.UnregisterAllCallback(this);
		}

		private void OnApplicationQuit()
		{
			bNoNewAnyThing = true;
		}

		protected virtual void OnDispose()
		{
			// for override
		}

		/********************************
		 * 初始化
		 * *****************************/

		private int sortIdx		= -1;
		private int sceneType	= -1;

		protected virtual void OnInitialUI()
		{
			// for overrdie
		}

		public void InitialUI(int idx, int sceneType)
		{
			this.sortIdx		= idx;
			this.sceneType		= sceneType;

			SceneController.Instance.RegisterFadeCallback(sceneType, TriggerToFadeOut, IsFadeOutFinish);

			OnInitialUI();
		}
		public int SortIdx { get => sortIdx; set => sortIdx = value; }

		/***************************************
		* 場景切換等待UI流程
		* *************************************/
		private void TriggerToFadeOut()
		{
			OnTriggerToFadeOut();
		}

		protected virtual void OnTriggerToFadeOut()
		{
			// for override
		}

		public bool IsFadeOutFinish()
		{
			return OnIsFadeOutFinish();
		}

		protected virtual bool OnIsFadeOutFinish()
		{
			// for override
			return true;
		}
	}
}

