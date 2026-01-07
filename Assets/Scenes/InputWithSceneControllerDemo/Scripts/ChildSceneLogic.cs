using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XPlan.InputMode;
using XPlan.Observe;
using XPlan.Scenes;
using XPlan.Utility;

namespace XPlan.Demo.InputMode
{ 
    public class ChildSceneLogic : NotifyMonoBehaviour
    {
        [SerializeField] string sceneName   = "";
        [SerializeField] string nextScene   = "";

        // Start is called before the first frame update
        private new void Awake()
        {
            base.Awake();

            Debug.Log("It is " + sceneName);

            GameViewSizeForce.EnsureAndUseFixed("XPlan.Demo", 1920, 1080);
        }

        [NotifyHandler]
        public void OnXInputActionMsg(XInputActionMsg msg)
        {
            if (!SceneController.Instance.IsLastScene(sceneName))
            {
                return;
            }

            switch (msg.inputAction)
			{
                case "LoadScene":
                    SceneController.Instance.ChangeTo(nextScene);                     
                    break;
                case "UnloadScene":                        
                    SceneController.Instance.BackFrom();
                    break;
            }
        }
    }
}