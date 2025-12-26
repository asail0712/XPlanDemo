using System.Threading.Tasks;

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
