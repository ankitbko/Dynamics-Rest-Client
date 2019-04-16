namespace Microsoft.Dynamics.CrmRestClient
{
	using System;
	using System.Collections.Generic;

	/// <summary>
	/// Interface for the metadata base for internal use (for Crm entities and attributes)
	/// </summary>
	public interface IMetadataBase
	{
		/// <summary>
		/// The metadata identifier.
		/// </summary>
		Guid Id { get; }

		/// <summary>
		/// The logical name the metadata is identified with.
		/// </summary>
		string LogicalName { get; }

		/// <summary>
		/// The display name for the metadata (as a localized label).
		/// </summary>
		IDisplayName DisplayName { get; }

		/// <summary>
		/// The English-language version of the Display Name of the attribute metadata.
		/// </summary>
		string EnglishDisplayName { get; }

        /// <summary>
        /// Indexer to retrieve value for a field.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>Value of the field specified in <paramref name="fieldName"/></returns>
		string this[string fieldName] { get; }

        /// <summary>
        /// Get list of fields in the metadata.
        /// </summary>
        /// <returns>Collection of field names</returns>
		IEnumerable<string> GetFieldNames();

        /// <summary>
        /// Get the value for <paramref name="fieldName"/> if exists, otherwise return <paramref name="defaultValue"/>
        /// </summary>
        /// <typeparam name="ResultType">Type of the value.</typeparam>
        /// <param name="fieldName">Name of the field present in the metadata.</param>
        /// <param name="defaultValue">Default value to return if <paramref name="fieldName"/> is not present.</param>
        /// <returns>Value for specified field/returns>
		ResultType GetFieldValue<ResultType>(string fieldName, ResultType defaultValue = default(ResultType));
	}
}
