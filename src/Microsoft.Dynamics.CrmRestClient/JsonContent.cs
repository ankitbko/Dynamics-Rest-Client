namespace Microsoft.Dynamics.CrmRestClient
{
	using Newtonsoft.Json;
	using System.Net.Http;
	using System.Text;

	public sealed class JsonContent : StringContent
	{
		public JsonContent(string content) : base(content, Encoding.UTF8, "application/json")
		{
			//
		}

		public JsonContent(object content) : this(JsonConvert.SerializeObject(content))
		{
			//
		}
	}
}
