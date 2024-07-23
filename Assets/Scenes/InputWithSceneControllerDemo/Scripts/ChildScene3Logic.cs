using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XPlan.Scenes;
using XPlan.InputMode;
using XPlan.Observe;

namespace XPlan.Demo.Input
{ 
    public class ChildScene3Logic : MonoBehaviour, INotifyReceiver
    {
        public Func<string> LazyGroupID
        {
            get
            {
                return () => "Scene3";
            }
            set
            {
                LazyGroupID = value;
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            Debug.Log("It is Child3 Scene");

            NotifySystem.Instance.RegisterNotify<InputActionMsg>(this, (msgReceiver) =>
            {
                InputActionMsg msg = msgReceiver.GetMessage<InputActionMsg>();

                switch(msg.inputAction)
				{
                    case "LoadScene":
                        Debug.Log("Cant Load Other Scene");                      
                        break;
                    case "UnloadScene":
                        Debug.Log("Unload Child3 Scene");
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