using System;
using System.IO;
using System.Net.Mail;
using System.Text.RegularExpressions;

using UnityEngine;

namespace XPlan.Extensions
{
    public static class StringExtensions
    {
        public static bool ChangeToPhoneNumStyle(this string numStr, ref string phoneNumberStr)
        {
            if(numStr.Length < 9 || numStr.Length > 10)
			{
                return false;
			}

            if(!numStr.StartsWith('0'))
			{
                return false;
			}

            phoneNumberStr = numStr.Insert(numStr.Length - 4, "-");
            phoneNumberStr = phoneNumberStr.Insert(2, ")");
            phoneNumberStr = phoneNumberStr.Insert(0, "(");

            return true;
        }

        public static bool IsValidEmail(this string email, bool bAllowEmpty = false)
        {
            if(email.Length == 0)
			{
                return bAllowEmpty;
			}

            try
            {
                var mailAddress = new MailAddress(email);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        public static bool IsValidPhoneNumber(this string phoneNumber)
        {
            // 使用正則表達式來檢查電話號碼是否有效
            // 這個範例使用簡單的規則來檢查電話號碼：
            // 必須以數字開頭，總長度為10或11個字符
            string pattern = @"^\d{10,11}$";

            // 使用Regex.IsMatch方法進行匹配
            return Regex.IsMatch(phoneNumber, pattern);
        }

        public static bool IsValidPassword(this string password, int min = 8, int max = 16)
        {
            // 使用正則表達式來檢查密碼是否有效
            // 這個範例使用簡單的規則來檢查密碼：
            // 必須為8到16位英文字母和數字的組合
            string pattern = @"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d]{"+ min.ToString() + "," + max.ToString() + "}$";

            // 使用Regex.IsMatch方法進行匹配
            return Regex.IsMatch(password, pattern);
        }

        public static string GetFileNameFromUrl(this string url)
        {
            string fileName = url;

            // 使用 Uri 來解析 URL
            try
            { 
                Uri uri     = new Uri(url);
                // 使用 Path.GetFileName 取得檔案名稱部分
                fileName    = Path.GetFileName(uri.LocalPath);
            }
            catch(Exception e)
			{
                Debug.LogWarning(e.ToString());
			}

            return fileName;
        }

        public static Color HexToColor(this string hexColor)
        {
            if (hexColor.Length != 7 || hexColor[0] != '#')
            {
                throw new ArgumentException("Invalid hex color format");
            }

            // 解析 R、G、B 分量
            float red   = Convert.ToInt32(hexColor.Substring(1, 2), 16) / 255.0f;
            float green = Convert.ToInt32(hexColor.Substring(3, 2), 16) / 255.0f;
            float blue  = Convert.ToInt32(hexColor.Substring(5, 2), 16) / 255.0f;

            return new Color(red, green, blue, 1.0f);
        }

        public static Texture2D Base64ToTex(this string str)
        {
            byte[] imageData = Convert.FromBase64String(str);

            // 將二進制數據轉換為 Unity 的 Texture2D
            Texture2D beautyTexture = new Texture2D(2, 2);
            beautyTexture.LoadImage(imageData); // 載入圖片數據

            return beautyTexture;
        }

        public static DateTime UTCtoLocalTime(this string utcTimeStr)
        {
            long timestamp                  = long.Parse(utcTimeStr);
            DateTimeOffset dateTimeOffset   = DateTimeOffset.FromUnixTimeMilliseconds(timestamp);
            DateTime utcTime                = dateTimeOffset.UtcDateTime;
            DateTime localTime              = utcTime.ToLocalTime();

            return localTime;
        }
    }
}

