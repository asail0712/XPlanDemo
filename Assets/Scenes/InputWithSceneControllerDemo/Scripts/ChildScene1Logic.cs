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
    public class ChildScene1Logic : MonoBehaviour, INotifyReceiver
    {
		public Func<string> LazyGroupID 
        { 
            get
            {
                return () => "Scene1";
            }
            set
            {
                LazyGroupID = value;
            }
        }

		// Start is called before the first frame update
		void Start()
        {
            Debug.Log("It is Child1 Scene");

            NotifySystem.Instance.RegisterNotify<InputActionMsg>(this, (msgReceiver) =>
            {
                InputActionMsg msg = msgReceiver.GetMessage<InputActionMsg>();

                switch(msg.inputAction)
				{
                    case "LoadScene":
                        Debug.Log("Child1 Load Child2 Scene");
                        SceneController.Instance.ChangeTo("ChildScene2");
                        break;
                    case "UnloadScene":
                        Debug.Log("Cant Unload Child1 Scene");
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