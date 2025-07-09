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
		public GetWebRequest()
        {
			
		}

        override protected string GetRequestMethod()
        {
            return "GET";
        }
    }
}
