namespace Microsoft.Dynamics.CrmRestClient
{
	using System;

    /// <summary>
    /// Represents a reference to an entity.
    /// </summary>
	public interface IEntityReference
	{
        /// <summary>
        /// The identifier for the record.
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// Logical name of the record.
        /// </summary>
        string LogicalName { get; }
	}
}
