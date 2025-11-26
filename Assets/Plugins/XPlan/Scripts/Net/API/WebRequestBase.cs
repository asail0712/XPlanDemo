//using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

using XPlan.Utility;

namespace XPlan.Net
{
    public sealed class WebResponseData
    {
        public string ContentType { get; }
        public string Text { get; }
        public byte[] RawData { get; }

        public bool IsText      => !string.IsNullOrEmpty(Text);
        public bool IsBinary    => RawData != null && RawData.Length > 0;

        public WebResponseData(string contentType, string text, byte[] rawData)
        {
            ContentType = contentType;
            Text        = text;
            RawData     = rawData;
        }
    }

    public sealed class ApiResult<T>
    {
        public bool IsSuccess { get; }
        public T Data { get; }
        public string ErrorMessage { get; }

        private ApiResult(bool isSuccess, T data, string errorMessage)
        {
            IsSuccess       = isSuccess;
            Data            = data;
            ErrorMessage    = errorMessage;
        }

        public static ApiResult<T> Success(T data)
            => new ApiResult<T>(true, data, null);

        public static ApiResult<T> Fail(string errorMessage)
            => new ApiResult<T>(false, default, errorMessage);
    }

    public class WebRequestBase
    {
        private string apiUrl;
		private Dictionary<string, string> headers;
        private Dictionary<string, string> urlParams;
        private bool bWaitingNet;
		private bool bAllowSoftError;
		private int timeOut;

		public WebRequestBase()
        {
			headers			= new Dictionary<string, string>();
            urlParams		= new Dictionary<string, string>();
            bWaitingNet		= true;
            bAllowSoftError = false;
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
            bWaitingNet = b;
        }

        public void AllowSoftError()
        {
            bAllowSoftError = true;
		}

        public void SetTimeout(int timeOut)
        {
            this.timeOut = timeOut;
        }

        public void SendWebRequest(Action<ApiResult<WebResponseData>> finishAction)
		{
			MonoBehaviourHelper.StartCoroutine(SendWebRequest_Internal(finishAction));
		}

		private IEnumerator SendWebRequest_Internal(Action<ApiResult<WebResponseData>> finishAction)
        {
			string url                      = GetUrl();
			using (UnityWebRequest request  = new UnityWebRequest(url, GetRequestMethod()))
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


                        var payload = new WebResponseData(contentType, text, null);
                        var result  = ApiResult<WebResponseData>.Success(payload);

                        finishAction?.Invoke(result);
                    }
					else
					{
						// 處理二進位資料
						byte[] data = request.downloadHandler.data;
						LogSystem.Record($"接收到 {data.Length} 位元組的二進位資料");

                        var payload = new WebResponseData(contentType, null, data);
                        var result  = ApiResult<WebResponseData>.Success(payload);

                        finishAction?.Invoke(result);
					}
				}
				else if (bAllowSoftError)
				{
                    string errorMsg     = request.error ?? "Unknown error";
                    string responseBody = request.downloadHandler != null ? request.downloadHandler.text : string.Empty;

                    // 這裡把錯誤訊息塞進 ErrorMessage，Data = default
                    var result = ApiResult<WebResponseData>.Fail(
                        $"[{apiUrl}] error: {errorMsg}, body: {responseBody}"
                    );

                    finishAction?.Invoke(result);
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
