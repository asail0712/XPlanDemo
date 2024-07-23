using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XPlan.DebugMode;

namespace XPlan.Demo.APIDemo
{ 
    public class APIDemoSystem : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            APIManager.RequestTemperature((bSuccess, result)=> 
            {
                if(bSuccess)
				{
                    LogSystem.Record($"成功取得高雄前金區的氣溫，氣溫是 {result} 度");
				}
                else
				{
                    LogSystem.Record($"無法成功取到高雄前金區的氣溫，原因是 {result}", LogType.Warning);
                }
            });
        }
    }
}
