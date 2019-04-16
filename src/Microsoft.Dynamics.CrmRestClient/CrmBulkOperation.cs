namespace Microsoft.Dynamics.CrmRestClient
{
	using Newtonsoft.Json.Linq;
	using System;
	using System.Collections.Concurrent;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net.Http;
	using System.Threading;
	using System.Threading.Tasks;

	public class CrmBulkOperation : ILockable, IDisposable
	{
		private ConcurrentDictionary<int, HttpMessageContent> contents = null;
		private ICrmClient crmClient = null;
		private List<JObject> jsonResults = null;
		private HttpResponseMessage lastResponse = null;

		private Func<IEnumerable<KeyValuePair<int, CrmTransmission>>, IEnumerable<int>> completeProcessor = null; 

		public bool CompleteOnDisposeIfContentFound { get; set; }

		public int AutoCompleteOnCount { get; set; }

		private int contentCount = 0;
		public int ContentCount
		{
			get
			{
				return this.contentCount;
			}
		}

		private string batchId = string.Empty;
		public string BatchId
		{
			get
			{
				if (string.IsNullOrEmpty(this.batchId))
				{
					this.batchId = Guid.NewGuid().ToCleanString().Replace("-", string.Empty);
				}
				return this.batchId;
			}
			set
			{
				this.batchId = value;
			}
		}

		public CrmBulkOperation(ICrmClient crmClient, string batchId = null, int autoCompleteOnCount = 0, bool completeOnDisposeIfContentFound = true, Func<IEnumerable<KeyValuePair<int, CrmTransmission>>, IEnumerable<int>> completeProcessor = null)
		{
			this.crmClient = crmClient;
			this.batchId = batchId;
			this.contents = new ConcurrentDictionary<int, HttpMessageContent>();
			this.jsonResults = new List<JObject>();
			this.contentCount = 0;
			this.AutoCompleteOnCount = autoCompleteOnCount;
			this.CompleteOnDisposeIfContentFound = completeOnDisposeIfContentFound;
			this.completeProcessor = completeProcessor;
		}

		public void ResetContent()
		{
			this.batchId = string.Empty;
			this.contents.Clear();
			this.contentCount = 0;
		}

		public int AddContent(string url, string method = "POST", string body = "{}")
		{
			HttpRequestMessage request = new HttpRequestMessage(new HttpMethod(method), $"{this.crmClient.CrmOdataUrl}{url}");
            request.Version = new Version(1, 1); // HTTP/2 breaks
			request.Content = new JsonContent(body);
			HttpMessageContent content = new HttpMessageContent(request);
			content.Headers.Remove("Content-Type");
			content.Headers.Add("Content-Type", "application/http");
			content.Headers.Add("Content-Transfer-Encoding", "binary");
			this.ProceedWhenUnlocked();
			int index = (Interlocked.Increment(ref this.contentCount) - 1);
			this.contents.AddOrUpdate(index, content, (oldKey, oldValue) => content);
			if (this.AutoCompleteOnCount > 0 && this.contentCount >= this.AutoCompleteOnCount)
			{
				this.Complete().Wait();
			}
			return index;
		}

		public int AddCreate(string entitySetName, string jsonData)
		{
			return this.AddContent(
				CrmClient.BuildRequestUrl(
					entitySetName: entitySetName),
				"POST",
				jsonData);
		}

		public int AddDelete(string entitySetName, Guid itemId)
		{
			return this.AddContent(
				CrmClient.BuildRequestUrl(
					entitySetName: entitySetName,
					itemId: itemId), 
				"DELETE");
		}

		public int AddRead(string entitySetName, string subEntitySetName = null, Guid? itemId = null, string fetchXml = null, string select = null, string inlineCount = null, string filter = null, int? top = null, string expand = null, string expandSelect = null, string expandFilter = null)
		{
			return this.AddContent(
				CrmClient.BuildRequestUrl(
					entitySetName: entitySetName,
					subEntitySetName: subEntitySetName,
					itemId: itemId,
					fetchXml: fetchXml,
					select: select,
					inlineCount: inlineCount,
					filter: filter,
					expand: expand,
					expandSelect: expandSelect,
					expandFilter: expandFilter,
					top: top), 
				"GET");
		}

		public int AddUpdate(string entitySetName, Guid itemId, string jsonData)
		{
			return this.AddContent(
				CrmClient.BuildRequestUrl(
					entitySetName: entitySetName,
					itemId: itemId),
				"PATCH",
				jsonData);
		}

		public async Task<HttpResponseMessage> CompleteIf(bool condition)
		{
			if (condition)
			{
				return await this.Complete();
			}
			return null;
		}

		public async Task<HttpResponseMessage> Complete()
		{
			var lockKey = this.AcquireLock();
			try
			{
				this.jsonResults.Clear();
				this.lastResponse.Destroy();
				if (this.contentCount > 0)
				{
					this.lastResponse = await this.crmClient.SendBatchAsync(this.BatchId, this.contents.OrderBy(pair => pair.Key).Select(pair => pair.Value).ToArray());
					this.lastResponse.EnsureSuccessStatusCode();
					var provider = await this.lastResponse.Content.ReadAsMultipartAsync();
					provider.Contents.RunPerItem(
						async (content) =>
						{
							string result = await content.ReadAsStringAsync();
							try
							{
								result = result.Substring(result.IndexOf("{"), result.Length - result.IndexOf("{"));
							}
							catch
							{
								result = "{}";
							}
							JObject json = JObject.Parse(result);
							this.jsonResults.Add(json);
						});
					this.ReportResultsAndResetContent();
				}
			}
			finally
			{
				this.ReleaseLock(lockKey);
			}
			return this.lastResponse;
		}

		public void Dispose()
		{
			try
			{
				this.ProceedWhenUnlocked();
				if (this.CompleteOnDisposeIfContentFound && this.contentCount > 0)
				{
					this.Complete().Wait();
				}
				this.contents.Destroy();
				this.jsonResults.Destroy();
				this.lastResponse.Destroy();
			}
			catch { }
		}

		public async Task<IEnumerable<JObject>> GetResultsAsJObject(bool complete = true)
		{
			await this.CompleteIf(complete);
			return this.jsonResults;
		}

		public async Task<IEnumerable<JsonArrayResponse>> GetResultsAsJsonArrayResponse(Func<JToken, object, JToken> convert = null, object eventArgs = null, bool complete = true)
		{
			await this.CompleteIf(complete);
			return from result in this.jsonResults select result.ToJsonArrayResponse(convert, eventArgs);
		}

		public async Task<IEnumerable<JsonArrayResponse<DataType>>> GetResultsAsJsonArrayResponse<DataType>(Func<JToken, object, DataType> convert = null, object eventArgs = null, bool complete = true)
		{
			await this.CompleteIf(complete);
			return from result in this.jsonResults select result.ToJsonArrayResponse<DataType>(convert, eventArgs);
		}

		public async Task<IEnumerable<JsonArrayResponse<DataType, EventArgsType>>> GetResultsAsJsonArrayResponse<DataType, EventArgsType>(Func<JToken, EventArgsType, DataType> convert = null, EventArgsType eventArgs = default(EventArgsType), bool complete = true)
		{
			await this.CompleteIf(complete);
			return from result in this.jsonResults select result.ToJsonArrayResponse<DataType, EventArgsType>(convert, eventArgs);
		}

		private void ReportResultsAndResetContent()
		{
			if (this.completeProcessor != null)
			{
				var dictionary = new Dictionary<int, CrmTransmission>();
				int index = 0;
				this.contents.OrderBy(pair => pair.Key).RunPerItem(
					(request) =>
					{
						dictionary.Add(
							request.Key,
							new CrmTransmission(
								index: request.Key,
								requestContent: this.contents[request.Key],
								responseObject: (index < this.jsonResults.Count) ? this.jsonResults[index] : null));
						index++;
					});
				var reprocessIndexes = this.completeProcessor.Invoke(dictionary);
				List<HttpMessageContent> newContents = new List<HttpMessageContent>();
				this.contents
                    .Where((request) => reprocessIndexes?.Contains(request.Key) ?? false)
                    .RunPerItem((request) => newContents.Add(request.Value));
				this.ResetContent();
				newContents.RunPerItem(
					(content) =>
					{
						index = (Interlocked.Increment(ref this.contentCount) - 1);
						this.contents.AddOrUpdate(index, content, (oldKey, oldValue) => content);
					});
				newContents.Clear();
			}
			else
			{
				this.ResetContent();
			}
		}
	}
}
