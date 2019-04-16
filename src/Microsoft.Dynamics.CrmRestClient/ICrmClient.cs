using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Polly;

namespace Microsoft.Dynamics.CrmRestClient
{
    public interface ICrmClient
    {
        string CrmBaseUrl { get; }
        string CrmOdataUrl { get; }
        CrmBulkOperation CreateBulkOperation(string batchId = null, int autoCompleteOnCount = 0, bool completeOnDisposeIfContentFound = true, Func<IEnumerable<KeyValuePair<int, CrmTransmission>>, IEnumerable<int>> completeProcessor = null);
        Task<HttpResponseMessage> Delete(string entitySetName, Guid? itemId, Policy<HttpResponseMessage> retryPolicy = null);
        Task<HttpResponseMessage> ListAsStream(string entitySetName, string subEntitySetName = null, Guid? itemId = null, string fetchXml = null, string select = null, string inlineCount = null, string filter = null, int? top = null, string orderby = null, string expand = null, string expandSelect = null, string expandFilter = null, bool withAnnotations = false, Policy<HttpResponseMessage> retryPolicy = null);
        Task<JsonArrayResponse> List(string entitySetName, string subEntitySetName = null, Guid? itemId = null, string fetchXml = null, string select = null, string inlineCount = null, string filter = null, int? top = null, string orderby = null, string expand = null, string expandSelect = null, string expandFilter = null, bool withAnnotations = false, Func<JToken, object, JToken> convert = null, object eventArgs = null, JsonArrayResponse previousResponse = null, Policy<HttpResponseMessage> retryPolicy = null);
        Task<JsonArrayResponse<TData, TEventArgs>> List<TData, TEventArgs>(string entitySetName, string subEntitySetName = null, Guid? itemId = null, string fetchXml = null, string select = null, string inlineCount = null, string filter = null, int? top = null, string orderby = null, string expand = null, string expandSelect = null, string expandFilter = null, bool withAnnotations = false, Func<JToken, TEventArgs, TData> convert = null, TEventArgs eventArgs = default(TEventArgs), JsonArrayResponse<TData, TEventArgs> previousResponse = null, Policy<HttpResponseMessage> retryPolicy = null);
        Task<JsonArrayResponse<TData>> List<TData>(string entitySetName, string subEntitySetName = null, Guid? itemId = null, string fetchXml = null, string select = null, string inlineCount = null, string filter = null, int? top = null, string orderby = null, string expand = null, string expandSelect = null, string expandFilter = null, bool withAnnotations = false, Func<JToken, object, TData> convert = null, object eventArgs = null, JsonArrayResponse<TData> previousResponse = null, Policy<HttpResponseMessage> retryPolicy = null);
        Task<HttpResponseMessage> Patch(string entitySetName, string jsonData, Guid? itemId, bool withRepresentation = false, Policy<HttpResponseMessage> retryPolicy = null);
        Task<HttpResponseMessage> Post(string entitySetName, string jsonData, Guid? itemId = null, bool withRepresentation = false, Policy<HttpResponseMessage> retryPolicy = null);
        Task<HttpResponseMessage> SendBatchAsync(string batchId, params HttpContent[] contents);
        void RefreshHttpClient();
    }
}