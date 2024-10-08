using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XPlan.Scenes;
using XPlan.InputMode;
using XPlan.Observe;

namespace XPlan.Demo.InputMode
{ 
    public class ChildSceneLogic : MonoBehaviour, INotifyReceiver
    {
        [SerializeField] string sectorID    = "";
        [SerializeField] string sceneName   = "";
        [SerializeField] string nextScene   = "";

        public Func<string> GetLazyZoneID
        {
            get
            {
                return () => sectorID;
            }
            set
            {
                GetLazyZoneID = value;
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            Debug.Log("It is " + sceneName);

            NotifySystem.Instance.RegisterNotify<XInputActionMsg>(this, (msgReceiver) =>
            {
                XInputActionMsg msg = msgReceiver.GetMessage<XInputActionMsg>();

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