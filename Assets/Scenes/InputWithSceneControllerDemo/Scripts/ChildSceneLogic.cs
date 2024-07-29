using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XPlan.Scenes;
using XPlan.InputMode;
using XPlan.Observe;

namespace XPlan.Demo.Input
{ 
    public class ChildSceneLogic : MonoBehaviour, INotifyReceiver
    {
        [SerializeField] string groupID     = "";
        [SerializeField] string sceneName   = "";
        [SerializeField] string nextScene   = "";

        public Func<string> LazyGroupID
        {
            get
            {
                return () => groupID;
            }
            set
            {
                LazyGroupID = value;
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            Debug.Log("It is " + sceneName);

            NotifySystem.Instance.RegisterNotify<InputActionMsg>(this, (msgReceiver) =>
            {
                InputActionMsg msg = msgReceiver.GetMessage<InputActionMsg>();

                switch(msg.inputAction)
				{
                    case "LoadScene":
                        SceneController.Instance.ChangeTo(nextScene);                     
                        break;
                    case "UnloadScene":                        
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