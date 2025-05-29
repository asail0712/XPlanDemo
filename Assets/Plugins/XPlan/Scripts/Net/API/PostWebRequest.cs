using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

using XPlan.Utility;

namespace XPlan.Net
{
    public class PostWebRequest
	{
        private string apiUrl;
		private Dictionary<string, string> headers;
		private byte[] bodyRaw;
		private bool bWaitingNet;
		private bool bIgnoreError;

		public PostWebRequest()
        {
			headers			= new Dictionary<string, string>();
			bodyRaw			= null;
			bWaitingNet		= true;
			bIgnoreError	= false;
		}

		protected void SetUrl(string url)
        {
            apiUrl = url;
        }

		protected void AddHeader(string key, string value)
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

		protected void AppendData(WWWForm form)
		{
            AddHeader("Content-Type", form.headers["Content-Type"]);

            bodyRaw = form.data;
		}

		protected void AppendData(string text)
        {
			AddHeader("Content-Type", "application/json");

			bodyRaw = System.Text.Encoding.UTF8.GetBytes(text);
		}

		public void IgnoreError()
        {
			bIgnoreError = true;
		}

		public void SendWebRequest(Action<object> finishAction)
		{
			MonoBehaviourHelper.StartCoroutine(SendWebRequest_Internal(finishAction));
		}

		private IEnumerator SendWebRequest_Internal(Action<object> finishAction)
        {
			string url = apiUrl;

			using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
			{
				if(bodyRaw != null)
				{ 
					request.uploadHandler	= new UploadHandlerRaw(bodyRaw);
				}

				request.downloadHandler		= new DownloadHandlerBuffer();
				
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

				if (request.result == UnityWebRequest.Result.Success || bIgnoreError)
				{
					string contentType = request.GetResponseHeader("Content-Type");

					if (string.IsNullOrEmpty(contentType))
                    {
						LogSystem.Record($"發生錯誤但忽略 並傳回空字串");
						finishAction?.Invoke("");
					}
					else if (contentType.Contains("application/json") || contentType.Contains("text/"))
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
				else
				{
					// 輸出錯誤訊息
					LogSystem.Record($"{apiUrl} happen error with{request.error}");

					WebRequestHelper.TriggerError(apiUrl, request.downloadHandler.text);
				}
			}
		}

		public void SetWaiting(bool b)
        {
			bWaitingNet = b;
        }
    }
}
