namespace Microsoft.Dynamics.CrmRestClient
{
	using Newtonsoft.Json.Linq;
	using System;
	using System.Diagnostics.CodeAnalysis;
    using System.Net.Http;

    [SuppressMessage("Microsoft.Usage", "CA2240:ImplementISerializableCorrectly")]
	[Serializable]
	public class CrmWebApiException : Exception
	{
		private string stackTrace = string.Empty;

		public string Code { get; private set; }

		public string ExceptionType { get; private set; }

        public HttpResponseMessage Response { get; private set; }

        public override string StackTrace
		{
			get
			{
				return this.stackTrace;
			}
		}

		public CrmWebApiException(JToken error) :
            base(error.ReadChildAs("message", "An error occurred in the Crm Web Api call."), CrmWebApiException.GetInnerException(error))
		{
			this.Code = error.ReadChildAs("code", string.Empty);
			this.ExceptionType = error.ReadChildAs("type", string.Empty);
			this.stackTrace = error.ReadChildAs("stacktrace", string.Empty);
		}
        

        public CrmWebApiException(JToken error, HttpResponseMessage response)
            : this(error)
        {
            this.Response = response;
        }

        public CrmWebApiException(string message, HttpResponseMessage response)
            : base(message)
        {
            this.Response = response;
        }

        public CrmWebApiException(string message, Exception innerException)
            : base(message, innerException) { }

        private static CrmWebApiException GetInnerException(JToken error)
		{
			return error["innererror"] != null ? new CrmWebApiException(error["innererror"]) : null;
		}
	}
}
