using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XPlan.Observe;
using XPlan.Utility;

namespace XPlan.InputMode
{
    public class InputActionMsg : MessageBase
	{
        public string inputAction;

        public InputActionMsg(string inputAction, string groupID = "")
		{
            this.inputAction = inputAction;

            Send(groupID);
		}
    }
    
    public class InputManager : CreateSingleton<InputManager>
    {
        public Action<string, string> inputAction;

        static private List<InputSetting> settingStack = new List<InputSetting>();

        private bool bEnabled               = true;
        private Coroutine inputCoroutine    = null;

        protected override void InitSingleton()
        {
            inputCoroutine = StartCoroutine(GatherInput());
        }

        protected override void OnRelease(bool bAppQuit)
        {
            if(inputCoroutine != null)
            {
                StopCoroutine(inputCoroutine);
                inputCoroutine = null;
            }

            settingStack.Clear();

            base.OnRelease(bAppQuit);
        }

        public void RegisterSetting(InputSetting setting)
		{
            settingStack.Add(setting);
        }

        public void UnregisterSetting(InputSetting setting)
        {
            settingStack.Remove(setting);
        }

        public void EnableInput(bool b)
		{
            bEnabled = b;
        }

        private IEnumerator GatherInput()
		{
            while(true)
			{
                yield return null;

                // 記得要在yield return null 下面
                if (!bEnabled)
                {
                    continue;
                }

                if(settingStack.Count == 0)
				{
                    continue;
				}

                InputSetting setting            = settingStack[settingStack.Count - 1];
                List<InputInfo> inputInfoList   = setting.inputInfoList;

                foreach (InputInfo inputInfo in inputInfoList)
                {
                    bool bIsTrigger = inputInfo.IsTrigger(InputType.PressDown) 
                                    | inputInfo.IsTrigger(InputType.PressUp) 
                                    | inputInfo.IsTrigger(InputType.Hold);

                    if (bIsTrigger)
                    {
                        inputAction?.Invoke(inputInfo.actionStr, setting.msgGroupName);
                        new InputActionMsg(inputInfo.actionStr, setting.msgGroupName);

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
