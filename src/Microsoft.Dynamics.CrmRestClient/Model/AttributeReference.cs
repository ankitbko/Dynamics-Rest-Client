namespace Microsoft.Dynamics.CrmRestClient
{
	using System;

    /// <summary>
	/// Represents a reference to an attribute.
	/// </summary>
	public class AttributeReference : IAttributeReference
	{
        /// <summary>
		/// The Attribute identifier.
		/// </summary>
		public Guid Id { get; internal set; }

        /// <summary>
		/// The logical name of the attribute.
		/// </summary>
		public string LogicalName { get; internal set; }

        /// <summary>
        /// Represents a reference to an attribute.
        /// </summary>
        public AttributeReference()
		{
			//
		}

        /// <summary>
        /// Represents a reference to an attribute.
        /// </summary>
        /// <param name="logicalName">Logical name of the attribute.</param>
        /// <param name="id">Identitfier of the attribute.</param>
		public AttributeReference(string logicalName, Guid id)
		{
			this.LogicalName = logicalName;
			this.Id = id;
		}
	}
}
