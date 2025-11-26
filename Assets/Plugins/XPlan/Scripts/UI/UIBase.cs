using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;

using XPlan.Recycle;
using XPlan.UI.Fade;
using XPlan.Utility;

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

	public class UIBase : MonoBehaviour, IUIListener, IUIView
	{
        /********************************
		* Listen Handler Call
		* *****************************/
        private void Awake()
        {
		}

        /********************************
		* Listen Handler Call
		* *****************************/
        public void ListenCall(string id, ListenOption option, Action<UIParam[]> paramAction)
		{
            UIEventBus.ListenCall(id, this, option, (paramList) =>
			{
				paramAction?.Invoke(paramList);
			});
		}

		public void ListenCall<T>(string id, ListenOption option, Action<T> paramAction)
		{
            UIEventBus.ListenCall(id, this, option, (paramList) =>
			{
				paramAction?.Invoke(paramList[0].GetValue<T>());
			});
		}

		public void ListenCall(string id, ListenOption option, Action noParamAction)
		{
            UIEventBus.ListenCall(id, this, option, (paramList) =>
			{
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

        protected void RegisterTextSubmit(string uniqueID, TMP_InputField inputTxt, Action<string> onPress = null, bool bClearWhenPress = true)
        {
            inputTxt.onSubmit.AddListener((str) =>
            {
                if (bClearWhenPress)
                {
                    inputTxt.text = "";
                }

                DirectTrigger<string>(uniqueID, str, onPress);
            });
        }

        protected void RegisterTextSubmit(string uniqueID, InputField inputTxt, Action<string> onPress = null, bool bClearWhenPress = true)
        {
            inputTxt.onSubmit.AddListener((str) =>
            {
                if (bClearWhenPress)
                {
                    inputTxt.text = "";
                }

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
                UIEventBus.TriggerCallback<bool>(uniqueID, bOn, onPress);
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

                    UIEventBus.TriggerCallback<int>(uniqueID, idx, onPress);
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

        protected void RegisterPointTrigger(string uniqueID, Button button,
                                        Action<PointerEventData, PointEventTriggerHandler> onPress = null,
                                        Action<PointerEventData, PointEventTriggerHandler> onPull = null)
        {
			if(!button.TryGetComponent<PointEventTriggerHandler>(out PointEventTriggerHandler pointTrigger))
			{
				LogSystem.Record("按鈕上沒有PointEventTriggerHandler", LogType.Error);
				return;
			}

			RegisterPointTrigger(uniqueID, pointTrigger, onPress, onPull);            
        }

        protected void RegisterPointRoll(string uniqueID, Button button,
                                Action<PointerEventData, PointEventTriggerHandler> onEnter = null,
                                Action<PointerEventData, PointEventTriggerHandler> onExit = null)
        {
            if (!button.TryGetComponent<PointEventTriggerHandler>(out PointEventTriggerHandler pointTrigger))
            {
                LogSystem.Record("按鈕上沒有PointEventTriggerHandler", LogType.Error);
                return;
            }

            RegisterPointRoll(uniqueID, pointTrigger, onEnter, onExit);
        }

        protected void RegisterPointTrigger(string uniqueID, PointEventTriggerHandler pointTrigger,
												Action<PointerEventData, PointEventTriggerHandler> onPress = null,
												Action<PointerEventData, PointEventTriggerHandler> onPull = null)
		{
			pointTrigger.OnPointDown += (val) =>
			{
				onPress?.Invoke(val, pointTrigger);

                UIEventBus.TriggerCallback<bool>(uniqueID, true, null);
			};

			pointTrigger.OnPointUp += (val) =>
			{
				onPull?.Invoke(val, pointTrigger);

                UIEventBus.TriggerCallback<bool>(uniqueID, false, null);
			};
		}

        protected void RegisterPointRoll(string uniqueID, PointEventTriggerHandler pointTrigger,
                                    Action<PointerEventData, PointEventTriggerHandler> onEnter = null,
                                    Action<PointerEventData, PointEventTriggerHandler> onExit = null)
        {
            pointTrigger.OnPointEnter += (val) =>
            {
                onEnter?.Invoke(val, pointTrigger);

                UIEventBus.TriggerCallback<bool>(uniqueID, true, null);
            };

            pointTrigger.OnPointExit += (val) =>
            {
                onExit?.Invoke(val, pointTrigger);

                UIEventBus.TriggerCallback<bool>(uniqueID, false, null);
            };
        }

		protected void DirectTrigger<T>(string uniqueID, T param, Action<T> onPress = null)
		{
            UIEventBus.TriggerCallback<T>(uniqueID, param, onPress);
		}

		protected void DirectTrigger(string uniqueID, Action onPress = null)
		{
            UIEventBus.TriggerCallback(uniqueID, onPress);
		}

		/********************************
		* UI間的溝通
		* *****************************/
		protected void AddUIListener<T>(string uniqueID, Action<T> callback)
		{
            UIEventBus.RegisterCallback(uniqueID, this, (param) =>
			{
				callback?.Invoke(param.GetValue<T>());
			});
		}

		protected void AddUIListener(string uniqueID, Action callback)
		{
            UIEventBus.RegisterCallback(uniqueID, this, (dump) =>
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

            UIEventBus.UnlistenAllCall(this);
            UIEventBus.UnregisterAllCallback(this);
		}

		protected virtual void OnDispose()
		{
			// for override
		}

        /********************************
		 * 其他
		 * *****************************/
        protected string GetStr(string keyStr)
		{
			return StringTable.Instance.GetStr(keyStr);
		}

        protected string ReplaceStr(string keyStr, params string[] paramList)
        {
            return StringTable.Instance.ReplaceStr(keyStr, paramList);
        }

        protected void DefaultToggleBtns(Button[] btns)
        {
			for(int i = 0; i < btns.Length; ++i)
            {
				btns[i].gameObject.SetActive(i == 0);
            }
        }

		/********************************
		 * 工具
		 * *****************************/
		
        public void FadeInOutAlpha(CanvasGroup canvasGroup, float targetAlpha, float duration, Action finishAction = null)
		{
			MonoBehaviourHelper.StartCoroutine(FadeInOutAlpha_Internal(canvasGroup, targetAlpha, duration, finishAction));
		}

		private IEnumerator FadeInOutAlpha_Internal(CanvasGroup canvasGroup, float targetAlpha, float duration, Action finishAction = null)
		{
			float elapsedTime	= 0f;
			float startAlpha	= canvasGroup.alpha;

			while (elapsedTime < duration)
			{
				yield return null;
				elapsedTime			+= Time.deltaTime;
				float newAlpha		= Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / duration);
				canvasGroup.alpha	= newAlpha;
			}

			canvasGroup.alpha		= targetAlpha;

			finishAction?.Invoke();
		}

        /***************************************
		 * 實作IUIView
		 * *************************************/
        public int SortIdx { get; set; }

        public void RefreshLanguage(int currLang)
        {
            OnRefreshLanguage(currLang);
        }

        protected virtual void OnRefreshLanguage(int currLang)
        {

        }

        /***************************************
		 * UI Visible
		 * *************************************/
        public void ToggleUI(GameObject ui, bool bEnabled)
		{
			ViewVisibilityHelper.ToggleUI(ui, bEnabled);			
		}
	}
}

