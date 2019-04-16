namespace Microsoft.Dynamics.CrmRestClient
{
	using System;
	using System.Collections.Generic;

    /// <summary>
    /// Representation of the metadata of a Crm entity attribute.
    /// </summary>
	public interface IAttributeMetadata : IMetadataBase
	{
        /// <summary>
        /// Type of attribute.
        /// </summary>
		string AttributeType { get; }

        /// <summary>
        /// Format of attribute.
        /// </summary>
		string Format { get; }

        /// <summary>
        /// Collection of names of options (if present) in the attribute.
        /// </summary>
        /// <returns>Collection of option names</returns>
		IEnumerable<string> GetAttributeOptions();

        /// <summary>
        /// Returns a parser to parse attribute value based on <see cref="AttributeType"/>.
        /// </summary>
        /// <returns>Func which takes takes string as input and returns <see cref="IEnumerable<SearchHelper>"/></returns>
		Func<string, IEnumerable<SearchResult>> GetParser();
	}
}
