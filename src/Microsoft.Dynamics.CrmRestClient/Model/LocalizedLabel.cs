namespace Microsoft.Dynamics.CrmRestClient
{
	using Newtonsoft.Json.Linq;
	using System;
    using System.Xml.Linq;

    /// <summary>
    /// Represents a localized label
    /// </summary>
    [Serializable]
	public class LocalizedLabel : ILocalizedLabel
	{
		/// <summary>
		/// The display label in the laguage dictated by the language code
		/// </summary>
		public string Label { get; }

		/// <summary>
		/// An int representing the language of the label
		/// </summary>
		public int? LanguageCode { get; }

        /// <summary>
        /// Extract a Localized label from a JObject containing "Label" and "LanguageCode"
        /// </summary>
        /// <param name="localizedLabel">The JObject containing "Label" and "LanguageCode"</param>
        public LocalizedLabel(JObject localizedLabel)
		{
            this.Label = localizedLabel.ReadChildAs("Label", string.Empty);
            this.LanguageCode = localizedLabel.ReadChildAs<int?>("LanguageCode", null);
        }

		/// <summary>
		/// Set label and languageCode for Localized Label
		/// </summary>
		/// <param name="label">The display label in the laguage dictated by the language code</param>
		/// <param name="languageCode"> An int representing the language of the label</param>
		public LocalizedLabel(string label, int? languageCode)
		{
			this.Label = label;
			this.LanguageCode = languageCode;
		}

        /// <summary>
        /// Extract a Localized label from a XElement containing "description" and "languageCode as attributes."
        /// </summary>
        /// <param name="localizedLabel">The XElement containing "description" and "languageCode" as attributes.</param>
        public LocalizedLabel(XElement localizedLabel)
        {
            this.Label = localizedLabel.Attribute("description")?.Value;
            this.LanguageCode = (int?)localizedLabel.Attribute("languagecode");
        }
	}
}
