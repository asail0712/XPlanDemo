using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

using UnityEngine;

using XPlan.Net;

namespace XPlan.Demo.APIDemo
{
    
    public static class APIManager
    {
        static public void RequestTemperature(Action<bool, string> finishAction)
        {
            Task.Run(async () => 
            {
                // 調用氣象局的API
                NetJSDNResult<TemperatureResponse> netResult = await HttpHelper.Http.Get(APIDefine.WeatherUrl + APIDefine.TemperatureAPI)
                .AddHeader("Authorization", APIDefine.WeatherLicense)
                .AddHeader("limit", "1")
                .AddQuery("locationName", APIDefine.KaohsiungSection)
                .AddQuery("elementName", "AT")
                .SendAsyncJSDN<TemperatureResponse>();

                string temperatureStr   = "0";
                bool bResult            = false;

                if (netResult.bSuccess)
				{
                    TemperatureResponse response    = netResult.netData;
                    LocationInfo locInfo            = response.records.locations[0];
                    TimeInfo[] timeInfo             = locInfo.location[0].weatherElement[0].time;
                    int timeIdx                     = FindClosestTimeElement(timeInfo);

                    bResult         = true;
                    temperatureStr  = timeInfo[timeIdx].elementValue[0].value;
                }
                else
				{
                    bResult         = false;
                    temperatureStr  = netResult.errorResponse.CustomErrorMessage;
                }

                finishAction?.Invoke(bResult, temperatureStr);
            });
        }


        static private int FindClosestTimeElement(TimeInfo[] timeInfo)
        {
            // 获取当前时间
            DateTime currentTime = DateTime.Now;

            // 初始化最小时间差为一个足够大的值
            TimeSpan minTimeDifference = TimeSpan.MaxValue;

            // 保存最接近的时间元素索引
            int closestElementIndex = -1;

            // 遍历时间字符串数组
            for (int i = 0; i < timeInfo.Length; i++)
            {
                // 将字符串时间转换为 DateTime 对象
                DateTime elementTime = DateTime.ParseExact(timeInfo[i].dataTime, "yyyy-MM-dd HH:mm:ss", null);

                // 计算时间差
                TimeSpan timeDifference = currentTime - elementTime;

                // 如果时间差的绝对值小于当前最小时间差，则更新最小时间差和索引
                if (timeDifference.Duration() < minTimeDifference.Duration())
                {
                    minTimeDifference = timeDifference;
                    closestElementIndex = i;
                }
            }

            return closestElementIndex;
        }
    }
}