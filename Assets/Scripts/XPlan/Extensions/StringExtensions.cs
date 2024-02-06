using System;
using System.IO;
using System.Net.Mail;
using System.Text.RegularExpressions;

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
            // 使用 Uri 來解析 URL
            Uri uri = new Uri(url);
            // 使用 Path.GetFileName 取得檔案名稱部分
            string fileName = Path.GetFileName(uri.LocalPath);

            return fileName;
        }
    }
}

