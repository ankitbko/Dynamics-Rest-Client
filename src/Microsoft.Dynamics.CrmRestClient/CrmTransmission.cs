namespace Microsoft.Dynamics.CrmRestClient
{
	using Newtonsoft.Json.Linq;
	using System.Net.Http;

	public class CrmTransmission
	{
		public int Index { get; private set; }

		public HttpMessageContent RequestContent { get; private set; }

		public JObject ResponseJObject { get; private set; }

		public CrmTransmission(int index, HttpMessageContent requestContent, JObject responseObject)
		{
			this.Index = index;
			this.RequestContent = requestContent;
			this.ResponseJObject = responseObject;
		}
	}
}
