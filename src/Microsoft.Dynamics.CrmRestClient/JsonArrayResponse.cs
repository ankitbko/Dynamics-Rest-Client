namespace Microsoft.Dynamics.CrmRestClient
{
	using Newtonsoft.Json.Linq;
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;

	public class JsonArrayResponse : JsonArrayResponse<JToken>, IEnumerable<JToken>
	{
        public JsonArrayResponse(
            JObject response = null,
            Func<JToken, object, JToken> convert = null,
            object eventArgs = null,
            int page = 1) : base(response, convert ?? ((_, __) => response[JsonArrayResponse.JsonTokenNameValue] as JArray), eventArgs, page)
		{
		}
	}

	public class JsonArrayResponse<TData> : JsonArrayResponse<TData, object>, IEnumerable<TData>
	{
		public JsonArrayResponse(JObject response, int page = 1) : base(response, page)
		{
			//
		}

		public JsonArrayResponse(JObject response = null, Func<JToken, object, TData> convert = null, object eventArgs = null, int page = 1) : base(response, convert, eventArgs, page)
		{
			//
		}
	}

	public class JsonArrayResponse<TData, TEventArgs> : IEnumerable<TData>
	{
		protected const string ODataNextLinkField = "@odata.nextLink";
		protected const string JsonTokenNameError = "error";
		protected internal const string JsonTokenNameValue = "value";

		protected IEnumerable<TData> results = null;

		public string NextLink { get; private set; }

		public int Page { get; private set; }

		public CrmWebApiException Exception { get; protected set; }

		public JsonArrayResponse(JObject response, int page = 1)
		{
			this.Page = page;
			if (response == null)
			{
				this.NextLink = string.Empty;
			}
			else
			{
				this.NextLink = response.ReadChildAs(JsonArrayResponse<TData, TEventArgs>.ODataNextLinkField, string.Empty);
			}
		}

		public JsonArrayResponse(JObject response = null, Func<JToken, TEventArgs, TData> convert = null, TEventArgs eventArgs = default(TEventArgs), int page = 1) : this(response, page)
		{
			if (response == null || convert == null)
			{
				this.results = new List<TData>();
			}
			else
			{
				try
				{
					if (response[JsonArrayResponse.JsonTokenNameError] != null)
					{
						this.Exception = new CrmWebApiException(response[JsonArrayResponse.JsonTokenNameError]);
					}
				}
				catch { }
				try
				{
					if (response[JsonArrayResponse.JsonTokenNameValue] as JArray != null)
                    {
                        this.results =
                            from result in (response[JsonArrayResponse.JsonTokenNameValue] as JArray)
                            select convert.Invoke(result, eventArgs);
                    }
                    else
                    {
                        this.results = new List<TData> { convert.Invoke(response, eventArgs) };
                    }
				}
				catch
				{
					this.results = new List<TData>();
				}
			}
		}

		public void Clear()
		{
			this.results.Destroy();
			this.Exception.Destroy();
		}

		public IEnumerator<TData> GetEnumerator()
		{
			return this.results.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)this.results).GetEnumerator();
		}
	}
}
