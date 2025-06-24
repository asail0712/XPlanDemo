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
