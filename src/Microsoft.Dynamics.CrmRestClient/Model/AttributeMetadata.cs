namespace Microsoft.Dynamics.CrmRestClient
{
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    /// <summary>
    /// Representation of the metadata of a Crm entity attribute.
    /// </summary>
	public class AttributeMetadata : MetadataBase, IAttributeMetadata, IMetadataBase
	{
		private string attributeType = string.Empty;
		private string format = string.Empty;

        /// <summary>
        /// Type of attribute.
        /// </summary>
		public string AttributeType
		{
			get
			{
				if (string.IsNullOrEmpty(this.attributeType))
				{
					this.attributeType = this.MetadataJson.ReadChildAs("AttributeType", string.Empty);
				}
				return this.attributeType;
			}
		}

        /// <summary>
        /// Format of attribute.
        /// </summary>
		public string Format
		{
			get
			{
				if (string.IsNullOrEmpty(this.format))
				{
					this.format = this.MetadataJson.ReadChildAs("Format", string.Empty);
				}
				return this.format;
			}
		}

        /// <summary>
        /// Representation of the metadata of a Crm entity attribute.
        /// </summary>
        /// <param name="metadataJson">JSON to be used to load metadata.</param>
		public AttributeMetadata(JToken metadataJson) : base(metadataJson)
		{
			//
		}

        /// <summary>
        /// Collection of names of options (if present) in the attribute.
        /// </summary>
        /// <returns>Collection of option names</returns>
		public IEnumerable<string> GetAttributeOptions()
		{
			return this.GetFieldValue("OptionSet", new JArray())
				.Select(option => option.ReadChildAs<JObject>("DisplayName", null))
				.Where(displayNameMetadataJson => displayNameMetadataJson != null)
				.Select(displayNameMetadataJson => new DisplayName(displayNameMetadataJson))
				.Select(displayName => displayName.EnglishDisplayName);
		}

        /// <summary>
        /// Returns a parser to parse attribute value based on <see cref="AttributeType"/>.
        /// </summary>
        /// <returns>Func which takes takes string as input and returns <see cref="IEnumerable<SearchHelper>"/></returns>
		public Func<string, IEnumerable<SearchResult>> GetParser()
		{
			switch (this.AttributeType.ToLowerInvariant())
			{
				case "datetime":
					return (messageText) =>
					{
						var minSupportedValue = this.GetFieldValue("MinSupportedValue", DateTime.MinValue);
						var maxSupportedValue = this.GetFieldValue("MaxSupportedValue", DateTime.MaxValue);
						DateTime value = default(DateTime);
						if (!DateTime.TryParse(messageText, CultureInfo.InvariantCulture, DateTimeStyles.None, out value))
						{
							return SearchResult.GetNoResults();
						}
						if (value >= minSupportedValue && value <= maxSupportedValue)
						{
							return SearchResult.GetSuccessResult(messageText, value);
						}
						return SearchResult.GetNoResults();
					};
				case "string":
					return (messageText) =>
					{
						var maxLength = this.GetFieldValue("MaxLength", 0);
						if (maxLength <= 0 || (messageText ?? string.Empty).Length <= maxLength)
						{
							return SearchResult.GetSuccessResult(messageText, messageText);
						}
						return SearchResult.GetNoResults();
					};
			}
			return (messageText) => SearchResult.GetNoResults();
		}
	}
}
