namespace Microsoft.Dynamics.CrmRestClient
{
	using Newtonsoft.Json.Linq;
	using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
	using System.Threading;
	using System.Threading.Tasks;

	public static class CrmExtensions
	{
		public static JsonArrayResponse ToJsonArrayResponse(this JObject response, Func<JToken, object, JToken> convert = null, object eventArgs = null, int page = 1)
		{
			return new JsonArrayResponse(response, convert, eventArgs, page);
		}

		public static JsonArrayResponse<TData> ToJsonArrayResponse<TData>(this JObject response, Func<JToken, object, TData> convert = null, object eventArgs = null, int page = 1)
		{
			return new JsonArrayResponse<TData>(response, convert, eventArgs, page);
		}

		public static JsonArrayResponse<TData, TEventArgs> ToJsonArrayResponse<TData, TEventArgs>(this JObject response, Func<JToken, TEventArgs, TData> convert = null, TEventArgs eventArgs = default(TEventArgs), int page = 1)
		{
			return new JsonArrayResponse<TData, TEventArgs>(response, convert, eventArgs, page);
		}

        public static async Task<IEntityMetadata> GetEntityMetadataAsync(this CrmClient crmClient, string logicalName, bool expandAttributes = true)
        {
            return (await crmClient.List(
                entitySetName: "EntityDefinitions",
                filter: $"LogicalName eq '{logicalName}'",
                select: "MetadataId, LogicalName, DisplayName, EntitySetName, PrimaryIdAttribute, PrimaryNameAttribute",
                convert: (metadataJson, eventArgs) => { return new EntityMetadata(metadataJson); },
                expand: expandAttributes ? "Attributes" : null)).FirstOrDefault();
        }

        public static async Task<IEnumerable<IEntityMetadata>> GetAllEntityMetadataAsync(this CrmClient crmClient, bool expandAttributes = true)
        {
            JsonArrayResponse metadataRecordsResponse = null;
            List<IEntityMetadata> metadataRecords = null;
            do
            {
                metadataRecordsResponse = await crmClient.List(
                    entitySetName: "EntityDefinitions",
                    select: "MetadataId, LogicalName, DisplayName, EntitySetName, PrimaryIdAttribute, PrimaryNameAttribute",
                    previousResponse: metadataRecordsResponse);

                foreach (var metadataJson in metadataRecordsResponse)
                {
                    metadataRecords.Add(new EntityMetadata(metadataJson));
                }

                metadataRecordsResponse.Clear();
            }
            while (!string.IsNullOrEmpty(metadataRecordsResponse.NextLink));
            metadataRecordsResponse.Destroy();
            return metadataRecords;
        }

    }
}
