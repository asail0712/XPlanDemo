using Newtonsoft.Json;
using PCSC.Iso7816;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

using XPlan.Utility;

namespace XPlan.Net
{
    public class GetWebRequest : WebRequestBase
	{	
		public GetWebRequest()
        {
			
		}

        override protected string GetRequestMethod()
        {
            return "GET";
        }

        /***************************************
         * Send
         * ************************************/

        public async Task<TResponse> SendAsync<TResponse>()
        {
            return await SendAsync_Imp<TResponse>();
        }
    }
}
