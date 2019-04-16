namespace Microsoft.Dynamics.CrmRestClient
{
	using Newtonsoft.Json.Linq;
	using System;
	using System.Collections.Generic;

	/// <summary>
	/// Representation of the metadata of a Crm record.
	/// </summary>
	public abstract class MetadataBase : IMetadataBase
	{
		private JToken metadataJson = null;
		private Guid metadataId = Guid.Empty;
		private string logicalName = string.Empty;
		private DisplayName displayName = null;

		/// <summary>
		/// The internal JSON used to load the metadata.
		/// </summary>
		protected JToken MetadataJson
		{
			get
			{
				return this.metadataJson;
			}
		}

		/// <summary>
		/// The metadata identifier.
		/// </summary>
		public Guid Id
		{
			get
			{
				if (this.metadataId.Equals(Guid.Empty))
				{
					this.metadataId = this.metadataJson.ReadChildAs("MetadataId", Guid.Empty);
				}
				return this.metadataId;
			}
		}

		/// <summary>
		/// The logical name the metadata is identified with.
		/// </summary>
		public string LogicalName
		{
			get
			{
				if (string.IsNullOrEmpty(this.logicalName))
				{
					this.logicalName = this.metadataJson.ReadChildAs("LogicalName", string.Empty);
				}
				return this.logicalName;
			}
		}

		/// <summary>
		/// The display name for the metadata (as a localized label).
		/// </summary>
		public IDisplayName DisplayName
		{
			get
			{
				if (this.displayName == null)
				{
					this.displayName = new DisplayName(this.metadataJson.ReadChildAs<JObject>("DisplayName"));
				}
				return this.displayName;
			}
		}

		/// <summary>
		/// The English-language version of the Display Name of the metadata.
		/// </summary>
		public string EnglishDisplayName => this.DisplayName.EnglishDisplayName ?? this.LogicalName;

        /// <summary>
        /// Indexer to retrieve value for a field.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>Value of the field specified in <paramref name="fieldName"/></returns>
		public string this[string fieldName]
		{
			get
			{
				return this.MetadataJson.ReadChildAs(fieldName, string.Empty);
			}
		}

		/// <summary>
		/// Representation of the metadata of a Crm attribute or entity.
		/// </summary>
		/// <param name="metadataJson">JSON to be used to load the metadata.</param>
		protected MetadataBase(JToken metadataJson)
		{
			this.metadataJson = metadataJson;
		}

        /// <summary>
        /// Get list of fields in the metadata.
        /// </summary>
        /// <returns>Collection of field names</returns>
		public IEnumerable<string> GetFieldNames()
		{
			return this.MetadataJson.GetChildNames();
		}

        /// <summary>
        /// Get the value for <paramref name="fieldName"/> if exists, otherwise return <paramref name="defaultValue"/>
        /// </summary>
        /// <typeparam name="ResultType">Type of the value.</typeparam>
        /// <param name="fieldName">Name of the field present in the metadata.</param>
        /// <param name="defaultValue">Default value to return if <paramref name="fieldName"/> is not present.</param>
        /// <returns>Value for specified field/returns>
		public ResultType GetFieldValue<ResultType>(string fieldName, ResultType defaultValue = default(ResultType))
		{
			return this.MetadataJson.ReadChildAs(fieldName, defaultValue);
		}
	}
}
