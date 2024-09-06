using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using XPlan.InputMode;
using XPlan.Observe;

namespace XPlan.Demo.InputMode
{ 
    public class InputScene2Logic : MonoBehaviour, INotifyReceiver
    {
		public Func<string> GetLazySectorID
        { 
            get; set; 
        }

		// Start is called before the first frame update
		void Start()
        {
            Debug.Log("It is Input Scene2");

            NotifySystem.Instance.RegisterNotify<InputActionMsg>(this, (msgReceiver) =>
            {
                InputActionMsg msg = msgReceiver.GetMessage<InputActionMsg>();

                switch(msg.inputAction)
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