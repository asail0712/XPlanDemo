using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XPlan.Observe;
using XPlan.Utility;

namespace XPlan.InputMode
{
    [Serializable]
    [Flags]
	public enum InputType
	{
        None        = 0,
        PressDown   = 1 << 0,
        PressUp     = 1 << 1,
        Hold        = 1 << 2,
    }

    [Serializable]
    public class InputInfo
	{
        public string actionStr;
        public List<KeyCode> keyList;
        public InputType inputType;
        public float timeToDenind;
        public bool bModifierKeys;

        public bool IsTrigger(InputType type)
		{
            InputType currType = (inputType & type);

            if (currType == InputType.None)
			{
                return false;
			}

            // 複合鍵要使用And 所以把trigger改為 true
            bool bIsTrigger = bModifierKeys;// || keyList.Count > 0;
            
            // keyList.Count == 0 表示任一按鍵
            switch (currType)
			{
				case InputType.PressDown:
					if (keyList.Count == 0)
					{
						bIsTrigger |= Input.anyKeyDown;
					}
					else
					{
						foreach (KeyCode key in keyList)
						{
							if (bModifierKeys)
							{
								bIsTrigger &= Input.GetKeyDown(key);
							}
							else
							{
								bIsTrigger |= Input.GetKeyDown(key);
							}
						}
					}
					break;
				case InputType.PressUp:
					if (keyList.Count == 0)
					{
						//bIsTrigger |= Input.anyKey;
					}
					else
					{
						foreach (KeyCode key in keyList)
						{
							if (bModifierKeys)
							{
								bIsTrigger &= Input.GetKeyUp(key);
							}
							else
							{
								bIsTrigger |= Input.GetKeyUp(key);
							}
						}
					}
					break;
				case InputType.Hold:
                    if (keyList.Count == 0)
                    {
                        bIsTrigger |= Input.anyKey;
                    }
                    else
                    {
                        foreach (KeyCode key in keyList)
                        {
                            if (bModifierKeys)
                            {
                                bIsTrigger &= Input.GetKey(key);
                            }
                            else
                            {
                                bIsTrigger |= Input.GetKey(key);
                            }
                        }
                    }
                    break;
            }

            return bIsTrigger;
        }
	}


    public class InputSetting : MonoBehaviour
    {
        [SerializeField]
        public List<InputInfo> inputInfoList;

        [SerializeField]
        public string msgGroupName = "";

		private void Start()
		{
			InputManager.Instance.RegisterSetting(this);
		}

		void OnDestroy()
		{
			InputManager.Instance.UnregisterSetting(this);
		}
	}
}
