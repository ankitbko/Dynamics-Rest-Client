namespace Microsoft.Dynamics.CrmRestClient
{
	using System.Collections.Generic;

    /// <summary>
    /// Represents a single record in CRM.
    /// </summary>
	public interface IEntity : IEntityReference
	{
        /// <summary>
        /// Get list of fields in the entity.
        /// </summary>
        /// <returns></returns>
		IEnumerable<string> GetFieldNames();

        /// <summary>
        /// Get the value for <paramref name="fieldName"/> if exists, otherwise return <paramref name="defaultValue"/>
        /// </summary>
        /// <typeparam name="ResultType">Type of the value.</typeparam>
        /// <param name="fieldName">Name of the field present in the entity.</param>
        /// <param name="defaultValue">Default value to return if <paramref name="fieldName"/> is not present.</param>
        /// <returns>Value for specified field/returns>
		ResultType GetFieldValue<ResultType>(string fieldName, ResultType defaultValue = default(ResultType));
	}
}
