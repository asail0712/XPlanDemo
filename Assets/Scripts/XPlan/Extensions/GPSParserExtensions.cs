using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XPlan.Extensions
{
    public class GnrmcData
    {
        public bool bIgnore = true;
        public bool bActive = false;
        public DateTime dataTime;        
        public double[] latAndLng = new double[2];
        public double speed;
        //public double groundTrack;
    }

    public static class GPSParserExtensions
    {
        public static GnrmcData ParseGnrmc(this string gnrmcData)
        {
            // 參考資料
            // https://blog.csdn.net/return_oops/article/details/98480676
            // $GNRMC,024849.000,A,2504.7033,N,12134.5356,E,0.13,155.63,211223,,,A,V*0A

            GnrmcData result    = new GnrmcData();
            string[] fields     = gnrmcData.Split(',');

            if (fields[0] == "$GNRMC" && fields.Length > 2)
            {
                result.bIgnore  = false;
                result.bActive  = fields[2] == "A";
                result.dataTime = ParseTime(DateTime.Now.ToString("ddMMyy"), fields[1]);

                if (!result.bActive) 
                {
                    return result;
                }

                result.latAndLng[0] = ParseCoordinate(fields[3], fields[4]);
                result.latAndLng[1] = ParseCoordinate(fields[5], fields[6]);
                result.speed        = double.Parse(fields[7]) * 1.852;      // 將節轉換為km/h
                //result.groundTrack  = double.Parse(fields[8]);              // 北0度 東90度 南180度 西270度
			}

            return result;
        }

        private static double ParseCoordinate(string coordinate, string direction)
        {
            int len = 2;

            if(direction == "W" || direction == "E")
			{
                len = 3;
			}

            // 解析緯度或經度
            double value = double.Parse(coordinate.Substring(0, len)) + double.Parse(coordinate.Substring(len)) / 60.0;

            // 考慮南緯和西經的情況
            if (direction == "S" || direction == "W")
            {
                value = -value;
            }

            return value;
        }

        private static DateTime ParseTime(string date, string time, string timeZone = "Taipei Standard Time")
        {
            int day             = int.Parse(date.Substring(0, 2));
            int month           = int.Parse(date.Substring(2, 2));
            int year            = int.Parse("20" + date.Substring(4, 2));

            // 格式：hhmmss.sss
            int hours           = int.Parse(time.Substring(0, 2));
            int minutes         = int.Parse(time.Substring(2, 2));
            int seconds         = int.Parse(time.Substring(4, 2));
            int milliseconds    = int.Parse(time.Substring(7));

            TimeZoneInfo taipeiTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
            DateTime taipeiTime         = TimeZoneInfo.ConvertTimeFromUtc(new DateTime(year, month, day, hours, minutes, seconds, milliseconds), taipeiTimeZone);

            // 創建 DateTime 對象
            return taipeiTime;
        }
    }
}
