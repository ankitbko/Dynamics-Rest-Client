namespace Microsoft.Dynamics.CrmRestClient
{
	using System.Collections.Generic;

    /// <summary>
    /// Represents display name of Crm entity.
    /// </summary>
	public interface IDisplayName
	{
        /// <summary>
        /// Collection of labels per locale.
        /// </summary>
		IEnumerable<ILocalizedLabel> LocalizedLabels { get; }

        /// <summary>
		/// The language code for the english language
		/// </summary>
		string EnglishDisplayName { get; }
	}
}
