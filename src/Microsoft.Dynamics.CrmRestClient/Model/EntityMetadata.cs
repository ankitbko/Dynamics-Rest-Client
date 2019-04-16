namespace Microsoft.Dynamics.CrmRestClient
{
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Representation of the metadata of a Crm entity.
    /// </summary>
    public class EntityMetadata : MetadataBase, IEntityMetadata, IMetadataBase
    {
        private string entitySetName = string.Empty;
        private string primaryIdAttribute = string.Empty;
        private string primaryNameAttribute = string.Empty;

        [NonSerialized]
        private IDictionary<string, IAttributeMetadata> attributes = null;

        /// <summary>
        /// Get SetName for this entity.
        /// </summary>
        public string EntitySetName
        {
            get
            {
                if (string.IsNullOrEmpty(this.entitySetName))
                {
                    this.entitySetName = this.MetadataJson.ReadChildAs("EntitySetName", string.Empty);
                }
                return this.entitySetName;
            }
        }

        /// <summary>
        /// Name of the field containing ID of the entity.
        /// </summary>
        public string PrimaryIdAttribute
        {
            get
            {
                if (string.IsNullOrEmpty(this.primaryIdAttribute))
                {
                    this.primaryIdAttribute = this.MetadataJson.ReadChildAs("PrimaryIdAttribute", string.Empty);
                }
                return this.primaryIdAttribute;
            }
        }

        /// <summary>
        /// Name of the field containing Name of the entity.
        /// </summary>
        public string PrimaryNameAttribute
        {
            get
            {
                if (string.IsNullOrEmpty(this.primaryNameAttribute))
                {
                    this.primaryNameAttribute = this.MetadataJson.ReadChildAs("PrimaryNameAttribute", string.Empty);
                }
                return this.primaryNameAttribute;
            }
        }

        /// <summary>
        /// True if metadata contains attributes. False otherwise.
        /// </summary>
        public bool HasAttributes
        {
            get
            {
                return this.attributes != null && this.attributes.Any();
            }
        }

        /// <summary>
        /// Collection of Attributes in the metadata.
        /// </summary>
        public IDictionary<string, IAttributeMetadata> Attributes
        {
            get
            {
                return this.attributes;
            }
        }

        /// <summary>
        /// Representation of the metadata of a Crm entity.
        /// </summary>
        /// <param name="metadataJson">JSON to form metadata</param>
        /// <param name="attributeLoader">Function to load attribute if attribute is not present.</param>
        /// <param name="maxDegreeOfParallelism">Maximum number of concurrent tasks</param>
        public EntityMetadata(JToken metadataJson, Func<string, IAttributeMetadata> attributeLoader = null) : base(metadataJson)
        {
            Func<string, IAttributeMetadata> defaultAttributeLoader =
                (logicalName) =>
                {
                    var attributes = metadataJson.ReadChildAs("Attributes", new JArray()).Where(node => node.ReadChildAs("LogicalName", string.Empty).Equals(logicalName));
                    if (attributes.Any())
                    {
                        return new AttributeMetadata(attributes.FirstOrDefault());
                    }
                    return null;
                };

            this.attributes = new Dictionary<string, IAttributeMetadata>();
            var attributeList = metadataJson.ReadChildAs("Attributes", new JArray());

            foreach (var attributeMetadataJson in attributeList)
            {
                this.attributes[attributeMetadataJson.ReadChildAs("LogicalName", string.Empty)] = new AttributeMetadata(attributeMetadataJson);
            }
        }
    }
}
