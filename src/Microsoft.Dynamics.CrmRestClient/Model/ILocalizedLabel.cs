namespace Microsoft.Dynamics.CrmRestClient
{
    /// <summary>
    /// Represents a localized label.
    /// </summary>
	public interface ILocalizedLabel
	{
        /// <summary>
		/// The display label in the laguage dictated by the language code
		/// </summary>
		string Label { get; }

        /// <summary>
		/// An int representing the language of the label
		/// </summary>
		int? LanguageCode { get; }
	}
}
