using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using XPlan.DebugMode;

namespace XPlan.Demo.APIDemo
{ 
    public class APIDemoSystem : MonoBehaviour
    {
        [SerializeField] private Text showStrTxt;

        private string demoStr = "";

        // Start is called before the first frame update
        void Start()
        {
            APIManager.RequestTemperature((bSuccess, result)=> 
            {
                string resultStr    = "";
                LogType logType     = LogType.Log;

                if (bSuccess)
				{
                    resultStr   = $"成功取得高雄前金區的氣溫，氣溫是 {result} 度";
				}
                else
				{
                    resultStr   = $"無法成功取到高雄前金區的氣溫，原因是 {result}";
                    logType     = LogType.Warning;                    
                }

                LogSystem.Record(resultStr, logType);
                demoStr = resultStr;
            });
        }

		private void Update()
		{
            // 主線程才能刷新UI
            showStrTxt.text = demoStr;
        }
	}
}
