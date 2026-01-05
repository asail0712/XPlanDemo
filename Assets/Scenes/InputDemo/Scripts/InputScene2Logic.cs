using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using XPlan.InputMode;
using XPlan.Observe;
using XPlan.Utility;

namespace XPlan.Demo.InputMode
{ 
    public class InputScene2Logic : NotifyMonoBehaviour
    {
        private new void Awake()
        {
            base.Awake();

            GameViewSizeForce.EnsureAndUseFixed("XPlan.Demo", 1920, 1080);
        }

        [NotifyHandler]
        public void OnXInputActionMsg(XInputActionMsg msg)
        {
            switch (msg.inputAction)
            {
                case "LoadScene":
                    Debug.Log("Scene2 Load Scene3");
                    SceneManager.LoadScene("InputScene3");
                    break;
                case "UnloadScene":
                    Debug.Log("Unload Scene2");
                    SceneManager.LoadScene("InputScene1");
                    break;
            }
        }
    }
}