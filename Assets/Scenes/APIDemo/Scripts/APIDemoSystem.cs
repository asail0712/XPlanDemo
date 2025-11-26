using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using XPlan.Net;

namespace XPlan.Demo.APIDemo
{
    public class TemperatureAPI : GetWebRequest
    {
        public TemperatureAPI(Action<bool, string> finishAction)
        {
            SetUrl(APIDefine.WeatherUrl + APIDefine.TemperatureAPI);
            AddHeader("Authorization", APIDefine.WeatherLicense);
            AddHeader("limit", "1");
            AddUrlParam("locationName", APIDefine.QuerySection);
            AddUrlParam("elementName", "%E6%BA%AB%E5%BA%A6");

            SendWebRequest((result) =>
            {
                if(!result.IsSuccess)
                {
                    Debug.LogWarning(result.ErrorMessage);

                    return;
                }

                TemperatureResponse response    = JsonConvert.DeserializeObject<TemperatureResponse>(result.Data.Text);
                LocationInfo locInfo            = response.records.locations[0];
                TimeInfo[] timeInfo             = locInfo.location[0].weatherElement[0].time;
                int timeIdx                     = FindClosestTimeElement(timeInfo);
                string temperatureStr           = timeInfo[timeIdx].elementValue[0].Temperature;

                finishAction?.Invoke(true, temperatureStr);
            });
        }

        private int FindClosestTimeElement(TimeInfo[] timeInfo)
        {
            // 获取当前时间
            DateTime currentTime        = DateTime.Now;

            // 初始化最小时间差为一个足够大的值
            TimeSpan minTimeDifference  = TimeSpan.MaxValue;

            // 保存最接近的时间元素索引
            int closestElementIndex     = -1;

            // 遍历时间字符串数组
            for (int i = 0; i < timeInfo.Length; i++)
            {
                // 将字符串时间转换为 DateTime 对象
                DateTime elementTime    = DateTime.ParseExact(timeInfo[i].dataTime, "yyyy-MM-ddTHH:mm:sszzz", null);

                // 计算时间差
                TimeSpan timeDifference = currentTime - elementTime;

                // 如果时间差的绝对值小于当前最小时间差，则更新最小时间差和索引
                if (timeDifference.Duration() < minTimeDifference.Duration())
                {
                    minTimeDifference   = timeDifference;
                    closestElementIndex = i;
                }
            }

            return closestElementIndex;
        }
    }

    public class APIDemoSystem : MonoBehaviour
    {
        [SerializeField] private Text showStrTxt;

        // Start is called before the first frame update
        void Start()
        {
            WebRequestHelper.AddErrorDelegate(ErrorFadeback);

            new TemperatureAPI(Fadeback);
        }

        private void Fadeback(bool bSuccess, string content)
        {
            string resultStr    = "使用台灣氣象局的API，\n";
            LogType logType     = LogType.Log;

            if (bSuccess)
            {
                resultStr   += $"成功取得台北南港區的氣溫，氣溫是 {content} 度";
            }
            else
            {
                resultStr   += $"無法成功取到台北南港區的氣溫，原因是 {content}";
                logType     = LogType.Warning;
            }

            LogSystem.Record(resultStr, logType);
            showStrTxt.text = resultStr;
        }

        private void ErrorFadeback(string key, string errorBecuz, string errorContent)
        {
            Fadeback(false, errorContent);
        }
	}
}
