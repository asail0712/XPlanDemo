// ==============================================================================
// XPlan Framework
//
// Copyright (c) 2026 Asail
// All rights reserved.
//
// Author  : Asail0712
// Project : XPlan
// Description:
//     A modular framework for Unity projects, focusing on MVVM architecture,
//     runtime tooling, event-driven design, and extensibility.
//
// Contact : asail0712@gmail.com
// GitHub  : https://github.com/asail0712/XPlanDemo
//
// Unauthorized copying, modification, or distribution of this file,
// via any medium, is strictly prohibited without prior permission.
// ==============================================================================
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace XPlan.Net
{
    public class PostWebRequest : WebRequestBase
	{
		private byte[] bodyRaw;
	
		public PostWebRequest()
        {
			bodyRaw = null;
		}

        /***************************************
         * Send
         * ************************************/

        public async Task<TResponse> SendAsync<TRequest, TResponse>(TRequest request)
        {
            AppendData(JsonConvert.SerializeObject(request));

            return await SendAsync_Imp<TResponse>();
        }

        public async Task<TResponse> SendAsync<TResponse>(byte[] bytes)
        {
            AppendBinary(bytes);

            return await SendAsync_Imp<TResponse>();
        }

        public async Task<TResponse> SendAsync<TResponse>(WWWForm form)
        {
            AppendData(form);

            return await SendAsync_Imp<TResponse>();
        }

        public async Task<TResponse> SendAsync<TResponse>()
        {
            return await SendAsync_Imp<TResponse>();
        }

        /***************************************
         * 其他
         * ************************************/

        protected void AppendData(WWWForm form)
		{
            AddHeader("Content-Type", form.headers["Content-Type"]);

            bodyRaw = form.data;
		}
        
        protected void AppendBinary(byte[] data, string contentType = "application/octet-stream")
        {
            if (data == null || data.Length == 0)
                throw new ArgumentException("Binary data is null or empty.");

            AddHeader("Content-Type", contentType);
            bodyRaw = data;
        }

        protected void AppendData(string text)
        {
			AddHeader("Content-Type", "application/json");

			bodyRaw = System.Text.Encoding.UTF8.GetBytes(text);
		}

        override protected void SetUploadBuffer(UnityWebRequest request)
        {
            if (bodyRaw == null)
            {
                return;
            }

            request.uploadHandler = new UploadHandlerRaw(bodyRaw);           
        }

        override protected string GetRequestMethod()
        {
            return "POST";
        }
    }
}
