using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using XPlan.Scenes;
using XPlan.InputMode;
using XPlan.Observe;

namespace XPlan.Demo.Input
{ 
    public class ChildScene2Logic : MonoBehaviour, INotifyReceiver
    {
        public Func<string> LazyGroupID
        {
            get
            {
                return () => "Scene2";
            }
            set
            {
                LazyGroupID = value;
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            Debug.Log("It is Child2 Scene");

            NotifySystem.Instance.RegisterNotify<InputActionMsg>(this, (msgReceiver) =>
            {
                InputActionMsg msg = msgReceiver.GetMessage<InputActionMsg>();

                switch(msg.inputAction)
				{
                    case "LoadScene":
                        Debug.Log("Child2 Load Child3 Scene");
                        SceneController.Instance.ChangeTo("ChildScene3");
                        break;
                    case "UnloadScene":
                        Debug.Log("Unload Child2 Scene");
                        SceneController.Instance.BackFrom();
                        break;
                }
            });
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        void OnDestroy()
        {
            NotifySystem.Instance.UnregisterNotify(this);
        }
    }
}