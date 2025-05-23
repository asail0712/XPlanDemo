using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

using XPlan.Utility;

namespace XPlan.Net
{
    public class GetWebRequest
	{
        private string apiUrl;
		private Dictionary<string, string> headers;
		private Dictionary<string, string> urlParams;
		private bool bWaitingNet;
		private bool bIgnoreError;

		public GetWebRequest()
        {
			headers			= new Dictionary<string, string>();
			urlParams		= new Dictionary<string, string>();
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

		protected void AddUrlParam(string key, string value)
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

		public void IgnoreError()
		{
			bIgnoreError = true;
		}

		public void SendWebRequest(Action<object> finishAction)
		{
			MonoBehaviourHelper.StartCoroutine(SendWebRequest_Internal(finishAction));
		}

		private string GenerateUrl()
		{
			string url = apiUrl;

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

		private IEnumerator SendWebRequest_Internal(Action<object> finishAction)
        {
			string url = GenerateUrl();

			using (UnityWebRequest request = new UnityWebRequest(url, "GET"))
			{
				request.downloadHandler = new DownloadHandlerBuffer();

				foreach(var header in headers)
                {
					request.SetRequestHeader(header.Key, header.Value);
				}

				if (bWaitingNet)
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

					if (contentType.Contains("application/json") || contentType.Contains("text/"))
					{
						// 處理文字資料
						string text = request.downloadHandler.text;
						Debug.Log($"文字內容: {text}");

						finishAction?.Invoke(text);
					}
					else
					{
						// 處理二進位資料
						byte[] data = request.downloadHandler.data;
						Debug.Log($"接收到 {data.Length} 位元組的二進位資料");

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
