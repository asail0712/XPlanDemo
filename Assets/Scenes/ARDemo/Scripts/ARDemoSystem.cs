using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XPlan.AR;
using XPlan.Observe;
using XPlan.Utility;

namespace XPlan.Demo.AR
{
    [Serializable]
    public class ArModelInfo
	{
        [SerializeField] public string modelKey;
        [SerializeField] public GameObject modelPrefab;
	}

    public class ARDemoSystem : MonoBehaviour, INotifyReceiver
    {
        [SerializeField] private List<ArModelInfo> modelInfoList;

        private Dictionary<string, GameObject> modelDict;

        public Func<string> GetLazyZoneID { get; set; }

        // Start is called before the first frame update
        void Start()
        {
            modelDict = new Dictionary<string, GameObject>();

            NotifySystem.Instance.RegisterNotify<XARModelTrackMsg>(this, (msgReceiver) => 
            {
                XARModelTrackMsg msg = msgReceiver.GetMessage<XARModelTrackMsg>();

                int idx = modelInfoList.FindIndex((E04) => 
                {
                    return E04.modelKey == msg.imgKey;
                });

                if (!modelInfoList.IsValidIndex<ArModelInfo>(idx))
				{
                    return;
				}

                if(msg.bOn)
				{
                    if (modelDict.ContainsKey(msg.imgKey))
                    {
                        return;
                    }

                    ArModelInfo modelInfo   = modelInfoList[idx];
                    GameObject go           = GameObject.Instantiate(modelInfo.modelPrefab);
                    go.transform.position   = msg.trackPos;

                    modelDict.Add(modelInfo.modelKey, go);

                }
                else
				{
                    if (!modelDict.ContainsKey(msg.imgKey))
                    {
                        return;
                    }

                    GameObject go = modelDict[msg.imgKey];
                    GameObject.DestroyImmediate(go);
                    modelDict.Remove(msg.imgKey);
                }
            });
        }
    }
}
