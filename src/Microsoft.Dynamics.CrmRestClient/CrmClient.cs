namespace Microsoft.Dynamics.CrmRestClient
{
    using Newtonsoft.Json.Linq;
    using Polly;
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;

    public class CrmClient : IDisposable, ILockable, ICrmClient
    {
        public const string DefaultCrmApiVersion = "v8.1";

        private const int MaxRequestRetries = 3;

        private DateTimeOffset nextReconnect = DateTimeOffset.MinValue;
        private TimeSpan reconnectPeriod = TimeSpan.FromMinutes(15);
        internal string CrmApiUrl = string.Empty;

        private HttpClient httpClient = null;
        private IAuthenticator crmAuthenticator = null;
        private readonly Func<HttpClient> httpClientFactory = null;

        public CrmClient(string crmUrl,
            IAuthenticator crmAuthenticator,
            string crmApiVersion = CrmClient.DefaultCrmApiVersion,
            TimeSpan? reconnectPeriod = null,
            Func<HttpClient> httpClientFactory = null)
        {
            this.CrmBaseUrl = crmUrl;
            this.CrmApiUrl = $"{this.CrmBaseUrl}/api/data/{crmApiVersion}/";
            this.crmAuthenticator = crmAuthenticator;
            this.httpClientFactory = httpClientFactory;
            this.reconnectPeriod = reconnectPeriod ?? TimeSpan.FromMinutes(15);
        }

        public static CrmClient CreateClient(
            string crmUrl,
            IAuthenticator crmAuthenticator,
            string crmApiVersion = CrmClient.DefaultCrmApiVersion,
            TimeSpan? reconnectPeriod = null,
            Func<HttpClient> httpClientFactory = null)
        {
            return new CrmClient(crmUrl, crmAuthenticator, crmApiVersion, reconnectPeriod, httpClientFactory);
        }

        public string CrmBaseUrl { get; } = string.Empty;

        public string CrmOdataUrl => this.CrmApiUrl;

        public Task<HttpResponseMessage> Post(string entitySetName, string jsonData, Guid? itemId = null, bool withRepresentation = false, Policy<HttpResponseMessage> retryPolicy = null)
        {
            var policy = retryPolicy ?? Policy.NoOpAsync<HttpResponseMessage>();
            return policy.ExecuteAsync(async () => await this.PostAsync(
                BuildRequestUrl(
                    entitySetName: entitySetName,
                    itemId: itemId),
                jsonData,
                withRepresentation));
        }

        public Task<HttpResponseMessage> Patch(string entitySetName, string jsonData, Guid? itemId, bool withRepresentation = false, Policy<HttpResponseMessage> retryPolicy = null)
        {
            var policy = retryPolicy ?? Policy.NoOpAsync<HttpResponseMessage>();
            return policy.ExecuteAsync(async () => await this.PatchAsync(
                BuildRequestUrl(
                    entitySetName: entitySetName,
                    itemId: itemId),
                jsonData,
                withRepresentation));
        }

        public Task<HttpResponseMessage> Delete(string entitySetName, Guid? itemId, Policy<HttpResponseMessage> retryPolicy = null)
        {
            var policy = retryPolicy ?? Policy.NoOpAsync<HttpResponseMessage>();
            return policy.ExecuteAsync(async () => await this.DeleteAsync(
                BuildRequestUrl(
                    entitySetName: entitySetName,
                    itemId: itemId)));
        }

        public Task<HttpResponseMessage> ListAsStream(
            string entitySetName,
            string subEntitySetName = null,
            Guid? itemId = null,
            string fetchXml = null,
            string select = null,
            string inlineCount = null,
            string filter = null,
            int? top = null,
            string orderby = null,
            string expand = null,
            string expandSelect = null,
            string expandFilter = null,
            bool withAnnotations = false,
            Policy<HttpResponseMessage> retryPolicy = null)
        {
            return this.GetHttpResponseMessageWithoutContent(
                requestUrl: BuildRequestUrl(
                    entitySetName: entitySetName,
                    subEntitySetName: subEntitySetName,
                    itemId: itemId,
                    fetchXml: fetchXml,
                    select: select,
                    inlineCount: inlineCount,
                    filter: filter,
                    orderby: orderby,
                    expand: expand,
                    expandSelect: expandSelect,
                    expandFilter: expandFilter,
                    top: top),
                retryPolicy: retryPolicy);
        }

        public Task<JsonArrayResponse> List(
            string entitySetName,
            string subEntitySetName = null,
            Guid? itemId = null,
            string fetchXml = null,
            string select = null,
            string inlineCount = null,
            string filter = null,
            int? top = null,
            string orderby = null,
            string expand = null,
            string expandSelect = null,
            string expandFilter = null,
            bool withAnnotations = false,
            Func<JToken, object, JToken> convert = null, 
            object eventArgs = null,
            JsonArrayResponse previousResponse = null,
            Policy<HttpResponseMessage> retryPolicy = null)
        {
            return this.GetJsonArrayResponse(
                requestUrl: BuildRequestUrl(
                    entitySetName: entitySetName,
                    subEntitySetName: subEntitySetName,
                    itemId: itemId,
                    fetchXml: fetchXml,
                    select: select,
                    inlineCount: inlineCount,
                    filter: filter,
                    orderby: orderby,
                    expand: expand,
                    expandSelect: expandSelect,
                    expandFilter: expandFilter,
                    top: top),
                convert: convert,
                withAnnotations: withAnnotations,
                previousResponse: previousResponse,
                eventArgs: eventArgs,
                retryPolicy: retryPolicy);
        }

        public Task<JsonArrayResponse<TData>> List<TData>(
            string entitySetName,
            string subEntitySetName = null,
            Guid? itemId = null,
            string fetchXml = null,
            string select = null,
            string inlineCount = null,
            string filter = null,
            int? top = null,
            string orderby = null,
            string expand = null,
            string expandSelect = null,
            string expandFilter = null,
            bool withAnnotations = false,
            Func<JToken, object, TData> convert = null, 
            object eventArgs = null,
            JsonArrayResponse<TData> previousResponse = null,
            Policy<HttpResponseMessage> retryPolicy = null)
        {
            return this.GetJsonArrayResponse(
                requestUrl: BuildRequestUrl(
                    entitySetName: entitySetName,
                    subEntitySetName: subEntitySetName,
                    itemId: itemId,
                    fetchXml: fetchXml,
                    select: select,
                    inlineCount: inlineCount,
                    filter: filter,
                    orderby: orderby,
                    expand: expand,
                    expandSelect: expandSelect,
                    expandFilter: expandFilter,
                    top: top),
                convert: convert,
                withAnnotations: withAnnotations,
                previousResponse: previousResponse,
                eventArgs: eventArgs,
                retryPolicy: retryPolicy);
        }

        public Task<JsonArrayResponse<TData, TEventArgs>> List<TData, TEventArgs>(
            string entitySetName,
            string subEntitySetName = null,
            Guid? itemId = null,
            string fetchXml = null,
            string select = null,
            string inlineCount = null,
            string filter = null,
            int? top = null,
            string orderby = null,
            string expand = null,
            string expandSelect = null,
            string expandFilter = null,
            bool withAnnotations = false,
            Func<JToken, TEventArgs, TData> convert = null,
            TEventArgs eventArgs = default(TEventArgs),
            JsonArrayResponse<TData, TEventArgs> previousResponse = null,
            Policy<HttpResponseMessage> retryPolicy = null)
        {
            return this.GetJsonArrayResponse(
                requestUrl: BuildRequestUrl(
                    entitySetName: entitySetName,
                    subEntitySetName: subEntitySetName,
                    itemId: itemId,
                    fetchXml: fetchXml,
                    select: select,
                    inlineCount: inlineCount,
                    filter: filter,
                    orderby: orderby,
                    expand: expand,
                    expandSelect: expandSelect,
                    expandFilter: expandFilter,
                    top: top),
                convert: convert,
                withAnnotations: withAnnotations,
                previousResponse: previousResponse,
                eventArgs: eventArgs,
                retryPolicy: retryPolicy);
        }

        public CrmBulkOperation CreateBulkOperation(
            string batchId = null,
            int autoCompleteOnCount = 0,
            bool completeOnDisposeIfContentFound = true,
            Func<IEnumerable<KeyValuePair<int, CrmTransmission>>, IEnumerable<int>> completeProcessor = null)
        {
            return new CrmBulkOperation(
                crmClient: this,
                batchId: batchId,
                autoCompleteOnCount: autoCompleteOnCount,
                completeOnDisposeIfContentFound: completeOnDisposeIfContentFound,
                completeProcessor: completeProcessor);
        }

        private async Task<JsonArrayResponse> GetJsonArrayResponse(
            string requestUrl,
            Func<JToken, object, JToken> convert = null,
            bool withAnnotations = false,
            JsonArrayResponse previousResponse = null,
            object eventArgs = null,
            Policy<HttpResponseMessage> retryPolicy = null)
        {
            return new JsonArrayResponse(
                response: await this.GetJsonResponse(
                    requestUrl: previousResponse == null ? requestUrl : (string.IsNullOrEmpty(previousResponse.NextLink) ? requestUrl : previousResponse.NextLink),
                    withAnnotations: withAnnotations,
                    usingFullLink: previousResponse != null && !string.IsNullOrEmpty(previousResponse.NextLink),
                    retryPolicy: retryPolicy),
                convert: convert,
                eventArgs: eventArgs,
                page: previousResponse == null ? 1 : previousResponse.Page + 1);
        }

        private async Task<JsonArrayResponse<TData>> GetJsonArrayResponse<TData>(
            string requestUrl,
            Func<JToken, object, TData> convert,
            bool withAnnotations = false,
            JsonArrayResponse<TData> previousResponse = null,
            object eventArgs = null,
            Policy<HttpResponseMessage> retryPolicy = null)
        {
            return new JsonArrayResponse<TData>(
                response: await this.GetJsonResponse(
                    requestUrl: previousResponse == null ? requestUrl : (string.IsNullOrEmpty(previousResponse.NextLink) ? requestUrl : previousResponse.NextLink),
                    withAnnotations: withAnnotations,
                    usingFullLink: previousResponse != null && !string.IsNullOrEmpty(previousResponse.NextLink),
                    retryPolicy: retryPolicy),
                convert: convert,
                eventArgs: eventArgs,
                page: previousResponse == null ? 1 : previousResponse.Page + 1);
        }

        private async Task<JsonArrayResponse<TData, TEventArgs>> GetJsonArrayResponse<TData, TEventArgs>(
            string requestUrl,
            Func<JToken, TEventArgs, TData> convert,
            bool withAnnotations = false,
            JsonArrayResponse<TData, TEventArgs>
            previousResponse = null,
            TEventArgs eventArgs = default(TEventArgs),
            Policy<HttpResponseMessage> retryPolicy = null)
        {
            return new JsonArrayResponse<TData, TEventArgs>(
                 response: await this.GetJsonResponse(
                     requestUrl: previousResponse == null ? requestUrl : (string.IsNullOrEmpty(previousResponse.NextLink) ? requestUrl : previousResponse.NextLink),
                     withAnnotations: withAnnotations,
                     usingFullLink: previousResponse != null && !string.IsNullOrEmpty(previousResponse.NextLink),
                     retryPolicy: retryPolicy),
                 convert: convert,
                 eventArgs: eventArgs,
                 page: previousResponse == null ? 1 : previousResponse.Page + 1);
        }

        protected async virtual Task<JObject> GetJsonResponse(
            string requestUrl,
            bool withAnnotations = false,
            bool usingFullLink = false,
            Policy<HttpResponseMessage> retryPolicy = null)
        {
            var policy = retryPolicy ?? Policy.NoOpAsync<HttpResponseMessage>();
            var responseMessage = await policy.ExecuteAsync(async () => await GetAsync(requestUrl, withAnnotations, usingFullLink));
            var response = await responseMessage.Content.ReadAsStringAsync();
            return JObject.Parse(response);
        }

        protected virtual Task<HttpResponseMessage> GetHttpResponseMessageWithoutContent(
            string requestUrl,
            bool withAnnotations = false,
            bool usingFullLink = false,
            Policy<HttpResponseMessage> retryPolicy = null)
        {
            var policy = retryPolicy ?? Policy.NoOpAsync<HttpResponseMessage>();
            return policy.ExecuteAsync(async () => await GetAsync(requestUrl, withAnnotations, usingFullLink, HttpCompletionOption.ResponseHeadersRead));
        }

        private async Task<HttpRequestMessage> CreateHttpRequestMessage(HttpMethod httpMethod, Uri requestUri, HttpContent httpContent = null)
        {
            var request = new HttpRequestMessage(httpMethod, requestUri);
            if (httpContent != null)
            {
                request.Content = httpContent;
            }
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await this.crmAuthenticator.GetToken(this.CrmBaseUrl, false));
            return request;
        }

        internal static string BuildRequestUrl(
            string entitySetName,
            string subEntitySetName = null,
            Guid? itemId = null,
            string fetchXml = null,
            string select = null,
            string inlineCount = null,
            string filter = null,
            string orderby = null,
            string expand = null,
            int? top = null, string
            expandSelect = null,
            string expandFilter = null)
        {
            select = CrmClient.RemoveWhiteSpaces(select);

            string requestUrl = $"{entitySetName}";
            List<string> queryParameters = new List<string>();
            Func<string, string, bool, bool> queryIfProvided =
                (format, parameter, escaped) =>
                {
                    if (!string.IsNullOrEmpty(parameter))
                    {
                        queryParameters.Add(string.Format(format, escaped ? Uri.EscapeDataString(parameter) : parameter));
                        return true;
                    }
                    return false;
                };
            requestUrl += (itemId == null) ? string.Empty : $"({((Guid)itemId).ToCleanString()})";
            requestUrl += string.IsNullOrEmpty(subEntitySetName) ? string.Empty : $"/{subEntitySetName}";

            queryIfProvided.Invoke("fetchXml={0}", fetchXml, true);
            queryIfProvided.Invoke("$select={0}", select, false);
            queryIfProvided.Invoke("$inlinecount={0}", inlineCount, false);
            queryIfProvided.Invoke("$orderby={0}", orderby, false);
            queryIfProvided.Invoke("$filter={0}", filter, false);
            queryIfProvided.Invoke("$top={0}", top == null ? string.Empty : top.ToString(), false);

            if (!string.IsNullOrEmpty(expand))
            {
                if (!string.IsNullOrEmpty(expandSelect) || !string.IsNullOrEmpty(expandFilter))
                {
                    if (string.IsNullOrEmpty(expandSelect))
                    {
                        expand = $"{expand}($filter={expandFilter})";
                    }
                    else if (string.IsNullOrEmpty(expandFilter))
                    {
                        expand = $"{expand}($select={expandSelect})";
                    }
                    else
                    {
                        expand = $"{expand}($select={expandSelect}&$filter={expandFilter})";
                    }
                }
                queryParameters.Add($"$expand={expand}");
            }

            if (queryParameters.Count > 0)
            {
                requestUrl = $"{requestUrl}?{string.Join("&", queryParameters)}";
            }

            queryParameters.Destroy();
            return requestUrl;
        }

        protected virtual HttpClient GetHttpClient(bool forceRefresh = false)
        {
            var lockKey = this.AcquireLock();
            try
            {
                if (forceRefresh || this.httpClient == null || DateTimeOffset.Now >= this.nextReconnect)
                {
                    if (httpClientFactory != null)
                    {
                        this.httpClient = httpClientFactory.Invoke();
                    }
                    else
                    {
                        this.httpClient = new HttpClient() { Timeout = this.reconnectPeriod };
                        this.httpClient.BaseAddress = new Uri(this.CrmBaseUrl);
                    }
                    this.nextReconnect = DateTimeOffset.Now + this.reconnectPeriod;
                }
                return this.httpClient;
            }
            finally
            {
                this.ReleaseLock(lockKey);
            }
        }

        public void RefreshHttpClient()
        {
            this.GetHttpClient(forceRefresh: true);
        }

        public void Dispose()
        {
            this.httpClient.Destroy();
        }

        public async Task<HttpResponseMessage> SendBatchAsync(string batchId, params HttpContent[] contents)
        {
            using (var batchContent = new MultipartContent("mixed", $"batch_{batchId}"))
            {
                contents.RunPerItem((content) => batchContent.Add(content));
                using (var batchRequest = await this.CreateHttpRequestMessage(HttpMethod.Post, new Uri($"{this.CrmApiUrl}$batch")))
                {
                    batchRequest.Content = batchContent;
                    var response = await this.GetHttpClient().SendAsync(batchRequest);
                    response.EnsureSuccessStatusCode();
                    return response;
                }
            }
        }

        private async Task<HttpResponseMessage> GetAsync(string requestUrl, bool withAnnotations = false, bool usingFullLink = false, HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead)
        {
            var request = usingFullLink
                    ? await this.CreateHttpRequestMessage(HttpMethod.Get, new Uri(requestUrl, UriKind.Absolute))
                    : await this.CreateHttpRequestMessage(HttpMethod.Get, new Uri($"{this.CrmApiUrl}{requestUrl}", UriKind.Absolute));

            if (withAnnotations)
            {
                request.Headers.Add("Prefer", "odata.include-annotations=\"*\"");
            }

            var responseMessage = await this.GetHttpClient().SendAsync(request, completionOption);

            if (!responseMessage.IsSuccessStatusCode)
            {
                this.GetHttpClient(forceRefresh: true);
                var error = await responseMessage.Content.ReadAsStringAsync();
                if (error.IsJSON() && JObject.Parse(error)["error"] != null)
                {
                    throw new CrmWebApiException(JObject.Parse(error)["error"], responseMessage);
                }
                else
                {
                    throw new CrmWebApiException(error, responseMessage);
                }
            }

            return responseMessage;
        }

        private async Task<HttpResponseMessage> DeleteAsync(string requestUrl)
        {
            var request = await this.CreateHttpRequestMessage(HttpMethod.Delete, new Uri($"{this.CrmApiUrl}{requestUrl}"));
            return await this.GetHttpClient().SendAsync(request);
        }

        private async Task<HttpResponseMessage> PatchAsync(string requestUrl, string jsonData, bool withRepresentation)
        {
            var request = await this.CreateHttpRequestMessage(new HttpMethod("PATCH"), new Uri($"{this.CrmApiUrl}{requestUrl}"), new JsonContent(jsonData));
            AddRepresentationHeader(request, withRepresentation);
            return await this.GetHttpClient().SendAsync(request);
        }

        private async Task<HttpResponseMessage> PostAsync(string requestUrl, string jsonData, bool withRepresentation)
        {
            var request = await this.CreateHttpRequestMessage(HttpMethod.Post, new Uri($"{this.CrmApiUrl}{requestUrl}"), new JsonContent(jsonData));
            AddRepresentationHeader(request, withRepresentation);
            return await this.GetHttpClient().SendAsync(request);
        }

        private void AddRepresentationHeader(HttpRequestMessage request, bool withRepresentation)
        {
            if (withRepresentation)
                request.Headers.Add("Prefer", "return=representation");
        }

        private static string RemoveWhiteSpaces(string text) =>
            string.IsNullOrEmpty(text) ? text : text.Replace(" ", string.Empty);
    }
}
