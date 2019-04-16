namespace Microsoft.Dynamics.CrmRestClient
{
	using System;

    /// <summary>
    /// Represents a reference to an attribute.
    /// </summary>
	public interface IAttributeReference
	{
		/// <summary>
		/// The identifier for the attribute.
		/// </summary>
		Guid Id { get; }

		/// <summary>
		/// The logical name the attribute.
		/// </summary>
		string LogicalName { get; }
	}
}
