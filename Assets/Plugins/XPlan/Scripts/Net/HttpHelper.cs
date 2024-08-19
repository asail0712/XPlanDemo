using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using XPlan.Utility;

namespace XPlan.Net
{
    public class ErrorResponse
    {
        public int CustomErrorCode { get; set; }
        public string CustomErrorMessage { get; set; }
        public int Code { get; set; }
        public string Message { get; set; }
    }

	public class NetJSDNResult<T>
	{
        public bool bSuccess;
        public T netData;
        public ErrorResponse errorResponse;

        public async void WaitResult(HttpResponseMessage response)
		{
            bSuccess = response.IsSuccessStatusCode;

            string responseBody = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                netData = JsonConvert.DeserializeObject<T>(responseBody);
            }
            else
            {
				try 
                { 
                    errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(responseBody);

                    if(errorResponse == null)
					{
                        errorResponse = new ErrorResponse()
                        {
                            CustomErrorCode     = -1,
                            CustomErrorMessage  = "",
                            Code                = 500,
                            Message             = ""
                        };
                    }
                }
                catch(FormatException ex)
				{
                    errorResponse = new ErrorResponse()
                    {
                        CustomErrorCode     = -1,
                        CustomErrorMessage  = ex.Message,
                        Code                = 500,
                        Message             = ""
                    };
                }
            }
        }
    }

    public class NetParseResult<T>
    {
        public bool bSuccess;
        public object netData;
        public ErrorResponse errorResponse;

        public async void WaitResult(HttpResponseMessage response)
        {
            bSuccess = response.IsSuccessStatusCode;

            string responseBody = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                if(typeof(T) == typeof(bool))
                {
                    netData = Boolean.Parse(responseBody);
                }
                else if (typeof(T) == typeof(string))
				{
                    netData = responseBody;
                }
                else if (typeof(T) == typeof(int))
                {
                    netData = int.Parse(responseBody);
                }
                else if (typeof(T) == typeof(float))
                {
                    netData = float.Parse(responseBody);
                }
                // 之後可以再擴充
            }
            else
            {
				try 
                { 
                    errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(responseBody);

                    if (errorResponse == null)
                    {
                        errorResponse = new ErrorResponse()
                        {
                            CustomErrorCode     = -1,
                            CustomErrorMessage  = "",
                            Code                = 500,
                            Message             = ""
                        };
                    }
                }
                catch (FormatException ex)
                {
                    errorResponse = new ErrorResponse()
                    {
                        CustomErrorCode     = -1,
                        CustomErrorMessage  = ex.Message,
                        Code                = 500,
                        Message             = ""
                    };
                }
            }
        }

        public T GetData()
		{
            return (T)netData;
        }
    }

    public static class HttpHelper
    {
        public static bool bOfflineTest         = false;
        public static float simulationLateTime  = 0f;
        private static bool bWaitResponse       = false;
        
        public class Http
        {
            private HttpRequestMessage request;
            private HttpContent _body;
            private readonly HttpClient _client = new HttpClient();
            private readonly Dictionary<string, string> _queryStrings = new Dictionary<string, string>();

            public static Http Get(string url)
            {
                return new Http { request = new HttpRequestMessage(HttpMethod.Get, url) };
            }

            public static Http Post(string url)
            {
                return new Http { request = new HttpRequestMessage(HttpMethod.Post, url) };
            }

            public Http AddQuery(string key, string value)
            {
                _queryStrings.Add(key, value);
                return this;
            }

            public Http AddBodyWithJSON(string body)
            {
                _body = new StringContent(body, Encoding.UTF8, "application/json");
                return this;
            }

            public Http AddBodyWithXWWWFormUrlencoded(Dictionary<string, string> body)
            {
                _body = new FormUrlEncodedContent(body);

                return this;
            }

            public Http AddHeader(string key, string value)
            {
                request.Headers.Add(key, value);
                return this;
            }

            public void SendAsyncJSDN<T>(Action<NetJSDNResult<T>> finishAction)
            {
                UriBuilder uriBuilder   = new UriBuilder(request.RequestUri);
                List<string> query      = _queryStrings.Select(kvp => $"{kvp.Key}={kvp.Value}").ToList();
                uriBuilder.Query        = string.Join("&", query);
                request.RequestUri      = uriBuilder.Uri;

                if (_body != null)
                {
                    request.Content = _body;
                }

                MonoBehaviourHelper.StartCoroutine(SendAsyncJSDN_Internal<T>(finishAction));
            }
            
            private IEnumerator SendAsyncJSDN_Internal<T>(Action<NetJSDNResult<T>> finishAction)
			{
                NetJSDNResult<T> netResult  = new NetJSDNResult<T>();
                bWaitResponse               = true;

                yield return null;

                if (bOfflineTest)
                {
                    netResult.bSuccess = true;
                }
                else
                {
                    if (simulationLateTime > 0f)
                    {
                        yield return new WaitForSeconds(simulationLateTime * 1000);
                    }

                    Task<HttpResponseMessage> responseTask = _client.SendAsync(request);

                    yield return new WaitUntil(() => responseTask.IsCompleted);

                    HttpResponseMessage response = responseTask.Result;

                    netResult.WaitResult(response);
                }

                bWaitResponse = false;

                finishAction?.Invoke(netResult);
            }

			public async Task<NetJSDNResult<T>> SendAsyncJSDN<T>()
			{
				var uriBuilder = new UriBuilder(request.RequestUri);

				List<string> query = _queryStrings.Select(kvp => $"{kvp.Key}={kvp.Value}").ToList();
				uriBuilder.Query = string.Join("&", query);

				request.RequestUri = uriBuilder.Uri;
				// var request = new HttpRequestMessage(_method, uriBuilder.Uri);
				if (_body != null)
				{
					request.Content = _body;
				}

				NetJSDNResult<T> netResult = new NetJSDNResult<T>();

				bWaitResponse = true;

				if (bOfflineTest)
				{
					netResult.bSuccess = true;
				}
				else
				{
					if (simulationLateTime > 0f)
					{
						await Task.Delay((int)(simulationLateTime * 1000));
					}

					try
					{
						HttpResponseMessage response = await _client.SendAsync(request);
						netResult.WaitResult(response);
					}
					catch (Exception e)
					{
						Debug.LogWarning($"Request error : {e.Message} !!");
					}
				}

				bWaitResponse = false;

				return netResult;
			}

			public void SendAsyncParse<T>(Action<NetParseResult<T>> finishAction)
            {
                UriBuilder uriBuilder = new UriBuilder(request.RequestUri);
                List<string> query = _queryStrings.Select(kvp => $"{kvp.Key}={kvp.Value}").ToList();
                uriBuilder.Query = string.Join("&", query);
                request.RequestUri = uriBuilder.Uri;

                if (_body != null)
                {
                    request.Content = _body;
                }

                MonoBehaviourHelper.StartCoroutine(SendAsyncParse_Internal<T>(finishAction));
            }

            private IEnumerator SendAsyncParse_Internal<T>(Action<NetParseResult<T>> finishAction)
            { 
                NetParseResult<T> netResult = new NetParseResult<T>();
                bWaitResponse               = true;

                if (bOfflineTest)
				{
                    netResult.bSuccess = true;
                }
                else
                {
                    if (simulationLateTime > 0f)
                    {
                        yield return new WaitForSeconds(simulationLateTime * 1000);
                    }

                    Task<HttpResponseMessage> responseTask = _client.SendAsync(request);

                    yield return new WaitUntil(() => responseTask.IsCompleted);

                    HttpResponseMessage response = responseTask.Result;

                    netResult.WaitResult(response);
                }

                bWaitResponse = false;

                finishAction?.Invoke(netResult);
            }

			public async Task<NetParseResult<T>> SendAsyncParse<T>()
			{
				var uriBuilder = new UriBuilder(request.RequestUri);

				List<string> query = _queryStrings.Select(kvp => $"{kvp.Key}={kvp.Value}").ToList();
				uriBuilder.Query = string.Join("&", query);

				request.RequestUri = uriBuilder.Uri;
				// var request = new HttpRequestMessage(_method, uriBuilder.Uri);
				if (_body != null)
				{
					request.Content = _body;
				}

				NetParseResult<T> netResult = new NetParseResult<T>();

				bWaitResponse = true;

				if (bOfflineTest)
				{
					netResult.bSuccess = true;
				}
				else
				{
					if (simulationLateTime > 0f)
					{
						await Task.Delay((int)(simulationLateTime * 1000));
					}

					try
					{
						HttpResponseMessage response = await _client.SendAsync(request);
						netResult.WaitResult(response);
					}
					catch (Exception e)
					{
						Debug.LogWarning($"Request error : {e.Message} !!");
					}
				}

				bWaitResponse = false;

				return netResult;
			}
		}

        public static bool WaitResponse()
        {
            return bWaitResponse;
        }
    }
}


