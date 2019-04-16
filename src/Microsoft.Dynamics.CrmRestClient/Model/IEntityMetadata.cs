namespace Microsoft.Dynamics.CrmRestClient
{
    using System.Collections.Generic;

    /// <summary>
    /// Representation of the metadata of a Crm entity.
    /// </summary>
    public interface IEntityMetadata : IMetadataBase
	{
        /// <summary>
        /// Get SetName for this entity.
        /// </summary>
		string EntitySetName { get; }

        /// <summary>
        /// Name of the field containing ID of the entity.
        /// </summary>
		string PrimaryIdAttribute { get; }

        /// <summary>
        /// Name of the field containing Name of the entity.
        /// </summary>
		string PrimaryNameAttribute { get; }

        /// <summary>
        /// Collection of Attributes in the metadata.
        /// </summary>
		IDictionary<string, IAttributeMetadata> Attributes { get; }

        /// <summary>
        /// True if metadata contains attributes. False otherwise.
        /// </summary>
        bool HasAttributes { get; }
    }
}
