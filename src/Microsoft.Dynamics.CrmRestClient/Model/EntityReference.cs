namespace Microsoft.Dynamics.CrmRestClient
{
	using System;

    /// <summary>
    /// Represents a reference to an entity.
    /// </summary>
	public class EntityReference : IEntityReference
	{
        /// <summary>
        /// The identifier for the record.
        /// </summary>
		public Guid Id { get; internal set; }

        /// <summary>
        /// Logical name of the record.
        /// </summary>
		public string LogicalName { get; internal set; }

        /// <summary>
        /// Represents a reference to an entity.
        /// </summary>
        /// <param name="logicalName">Logical name of the record</param>
        /// <param name="id">Identifier of the record</param>
		public EntityReference(string logicalName, Guid id)
		{
			this.LogicalName = logicalName;
			this.Id = id;
		}
	}
}
