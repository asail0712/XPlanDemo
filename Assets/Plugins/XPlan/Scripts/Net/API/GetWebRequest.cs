using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

using XPlan.Utility;

namespace XPlan.Net
{
    public class GetWebRequest : WebRequestBase
	{
		private Dictionary<string, string> urlParams;
	
		public GetWebRequest()
        {
			urlParams = new Dictionary<string, string>();
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

		override protected string GetUrl()
		{
			string url = base.GetUrl();

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

        override protected string GetRequestMethod()
        {
            return "GET";
        }
    }
}
