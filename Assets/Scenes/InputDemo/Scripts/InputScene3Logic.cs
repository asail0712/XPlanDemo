using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using XPlan.InputMode;
using XPlan.Observe;

namespace XPlan.Demo.InputMode
{ 
    public class InputScene3Logic : MonoBehaviour, INotifyReceiver
    {
		public Func<string> GetLazySectorID
        { 
            get; set; 
        }

		// Start is called before the first frame update
		void Start()
        {
            Debug.Log("It is Input Scene3");

            NotifySystem.Instance.RegisterNotify<InputActionMsg>(this, (msgReceiver) =>
            {
                InputActionMsg msg = msgReceiver.GetMessage<InputActionMsg>();

                switch(msg.inputAction)
				{
                    case "LoadScene":
                        Debug.Log("Cant Load Other Scene");                      
                        break;
                    case "UnloadScene":
                        Debug.Log("Unload Scene3");
                        SceneManager.LoadScene("InputScene2");
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