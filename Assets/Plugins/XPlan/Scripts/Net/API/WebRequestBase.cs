//using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

using XPlan.Utility;

namespace XPlan.Net
{
    public class WebRequestBase
    {
        private string apiUrl;
		private Dictionary<string, string> headers;
        private Dictionary<string, string> urlParams;
        private bool bWaitingNet;
		private bool bIgnoreError;
		private int timeOut;

		public WebRequestBase()
        {
			headers			= new Dictionary<string, string>();
            urlParams		= new Dictionary<string, string>();
            bWaitingNet		= true;
			bIgnoreError	= false;
			timeOut			= 10;
        }

        public void AddHeader(string key, string value)
        {
            if (headers.ContainsKey(key))
            {
                headers[key] = value;
            }
            else
            {
                headers.Add(key, value);
            }
        }

        public void AddUrlParam(string key, string value)
        {
            value = UnityWebRequest.EscapeURL(value);

            if (urlParams.ContainsKey(key))
            {
                urlParams[key] = value;
            }
            else
            {
                urlParams.Add(key, value);
            }
        }

        public void SetUrl(string url)
        {
            this.apiUrl = url;
        }

        public string GetUrl()
        {
            string url = this.apiUrl;

            if (urlParams.Count > 0)
            {
                url += "?";

                foreach (var urlParam in urlParams)
                {
                    url += urlParam.Key + "=" + urlParam.Value;
                }
            }

            return url;
        }

        public void SetWaiting(bool b)
        {
            this.bWaitingNet = b;
        }

        public void IgnoreError()
        {
			this.bIgnoreError = true;
		}

        public void SetTimeout(int timeOut)
        {
            this.timeOut = timeOut;
        }

        public void SendWebRequest(Action<object> finishAction)
		{
			MonoBehaviourHelper.StartCoroutine(SendWebRequest_Internal(finishAction));
		}

		private IEnumerator SendWebRequest_Internal(Action<object> finishAction)
        {
			string url = GetUrl();

			using (UnityWebRequest request = new UnityWebRequest(url, GetRequestMethod()))
			{
				SetUploadBuffer(request);

				request.timeout			= timeOut;
                request.downloadHandler	= new DownloadHandlerBuffer();
				
				foreach (var header in headers)
                {
					request.SetRequestHeader(header.Key, header.Value);
				}

				if(bWaitingNet)
                {
					WebRequestHelper.IncreaseWaitingNum();
				}

				LogSystem.Record($"送出 {apiUrl} 資料");

				// 發送請求並等待回應
				yield return request.SendWebRequest();

				if (bWaitingNet)
				{
					WebRequestHelper.DecreaseWaitingNum();
				}

				if (request.result == UnityWebRequest.Result.Success)
				{
					string contentType = request.GetResponseHeader("Content-Type");

					if (contentType.Contains("application/json") || contentType.Contains("text/"))
					{
						// 處理文字資料
						string text = request.downloadHandler.text;
						LogSystem.Record($"文字內容: {text}");

						finishAction?.Invoke(text);
					}
					else
					{
						// 處理二進位資料
						byte[] data = request.downloadHandler.data;
						LogSystem.Record($"接收到 {data.Length} 位元組的二進位資料");

						finishAction?.Invoke(data);
					}
				}
				else if (bIgnoreError)
				{
                    finishAction?.Invoke("");
                }
				else
				{
					// 輸出錯誤訊息
					LogSystem.Record($"{apiUrl} happen error with{request.error}");

					WebRequestHelper.TriggerError(apiUrl, request.error, request.downloadHandler.text);
				}
			}
		}

		virtual protected void SetUploadBuffer(UnityWebRequest request)
		{
			// nothing to do
		}

        virtual protected string GetRequestMethod()
        {
			Debug.Assert(false);

            return "";
        }
    }
}
