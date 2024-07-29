using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using XPlan.InputMode;
using XPlan.Observe;

namespace XPlan.Demo.InputMode
{
    public class InputScene1Logic : MonoBehaviour, INotifyReceiver
    {
        public Func<string> LazyGroupID
        {
            get; set;
        }

        // Start is called before the first frame update
        void Start()
        {
            Debug.Log("It is Input Scene1");

            NotifySystem.Instance.RegisterNotify<InputActionMsg>(this, (msgReceiver) =>
            {
                InputActionMsg msg = msgReceiver.GetMessage<InputActionMsg>();

                switch (msg.inputAction)
                {
                    case "LoadScene":
                        Debug.Log("Scene1 Load Scene2");
                        SceneManager.LoadScene("InputScene2");
                        break;
                    case "UnloadScene":
                        Debug.Log("Cant Unload");
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