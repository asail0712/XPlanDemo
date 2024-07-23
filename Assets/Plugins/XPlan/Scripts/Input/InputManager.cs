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


    public class InputManager : SystemBase
    {
        [SerializeField]
        private List<InputInfo> inputInfoList;

        [SerializeField]
        private string msgGroupName = "";

        public Action<string> inputAction;

        static private List<MonoBehaviourHelper.MonoBehavourInstance> inputCoStack = new List<MonoBehaviourHelper.MonoBehavourInstance>();

        private bool bEnabled                                       = true;
        private MonoBehaviourHelper.MonoBehavourInstance inputCoIns = null;

        protected override void OnRelease(bool bAppQuit)
        {
            if(inputCoIns != null)
            {
                inputCoStack.Remove(inputCoIns);
                inputCoIns.StopCoroutine();
                inputCoIns = null;
            }

			// 因為要加新的，所以將原本的input停止
			if (inputCoStack.Count > 0)
			{
                MonoBehaviourHelper.MonoBehavourInstance coIns = inputCoStack[inputCoStack.Count - 1];
                if (coIns != null)
                {
                    coIns.StartCoroutine();
                }
                else // 避免場景被釋放掉 物件為null時，要將物件移除
                {
                    inputCoStack.RemoveAt(inputCoStack.Count - 1);
                }
            }

			base.OnRelease(bAppQuit);
        }

        protected override void OnInitialGameObject()
		{
            // 因為要加新的，所以將原本的Input停止
            if(inputCoStack.Count > 0)
			{
                MonoBehaviourHelper.MonoBehavourInstance coIns = inputCoStack[inputCoStack.Count - 1];
                if(coIns != null)
				{
                    coIns.StopCoroutine(false);
                }
                else // 避免場景被釋放掉 物件為null時，要將物件移除
                {
                    inputCoStack.RemoveAt(inputCoStack.Count - 1);
                }
            }

            // 觸發新的Input
            if(inputCoIns == null)
			{
                inputCoIns = MonoBehaviourHelper.StartCoroutine(GatherInput());
            }

            inputCoStack.Add(inputCoIns);
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

                foreach (InputInfo inputInfo in inputInfoList)
                {
                    bool bIsTrigger = inputInfo.IsTrigger(InputType.PressDown) 
                                    | inputInfo.IsTrigger(InputType.PressUp) 
                                    | inputInfo.IsTrigger(InputType.Hold);

                    if (bIsTrigger)
                    {
                        inputAction?.Invoke(inputInfo.actionStr);
                        new InputActionMsg(inputInfo.actionStr, msgGroupName);

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
