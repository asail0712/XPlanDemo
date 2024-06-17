using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XPlan.Utility;

namespace XPlan.InputMode
{
    [Serializable]
    [Flags]
    enum InputType
	{
        None        = 0,
        PressDown   = 1 << 0,
        PressUp     = 1 << 1,
        Hold        = 1 << 2,
    }

    [Serializable]
    class InputInfo
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


    public class InputManager : CreateSingleton<InputManager>
    {
        [SerializeField]
        List<InputInfo> inputInfoList;

        public Action<string> inputAction;

        private Coroutine inputCoroutine;

        // Start is called before the first frame update
        protected override void InitSingleton()
        {
        }

		void Start()
		{
            EnableInput(true);
        }

        public void EnableInput(bool b)
		{
            if(b)
			{
                if(inputCoroutine == null)
				{
                    inputCoroutine = StartCoroutine(GatherInput());
                }
			}
            else
			{
                if (inputCoroutine != null)
                {
                    StopCoroutine(inputCoroutine);
                    inputCoroutine = null;
                }
            }
		}

        private IEnumerator GatherInput()
		{
            while(true)
			{
                yield return null;

                foreach (InputInfo inputInfo in inputInfoList)
                {
                    bool bIsTrigger = inputInfo.IsTrigger(InputType.PressDown) 
                                    | inputInfo.IsTrigger(InputType.PressUp) 
                                    | inputInfo.IsTrigger(InputType.Hold);

                    if (bIsTrigger)
                    {
                        inputAction?.Invoke(inputInfo.actionStr);

                        if (inputInfo.timeToDenind > 0f)
                        { 
                            yield return new WaitForSeconds(inputInfo.timeToDenind);
                        }
                    }
                }
            }
        }
    }
}
