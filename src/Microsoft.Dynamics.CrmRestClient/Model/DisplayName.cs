namespace Microsoft.Dynamics.CrmRestClient
{
	using Newtonsoft.Json.Linq;
	using System.Collections.Generic;
	using System.Linq;
	using System;
    using System.Xml.Linq;

    /// <summary>
    /// Represents display name of Crm entity.
    /// </summary>
    [Serializable]
	public class DisplayName : IDisplayName
	{
		/// <summary>
		/// The language code for the english language
		/// </summary>
		public const int EnglishLanguageCode = 1033;

        /// <summary>
        /// Collection of labels per locale.
        /// </summary>
        public IEnumerable<ILocalizedLabel> LocalizedLabels { get;  private set; }

        /// <summary>
		/// The English language version of the Display Name.
		/// </summary>
		public string EnglishDisplayName =>
			this.LocalizedLabels
				.Where(localizedLabel => localizedLabel.LanguageCode == DisplayName.EnglishLanguageCode)
				.Select(localizedLabel => localizedLabel.Label)
				.FirstOrDefault();

        /// <summary>
        /// Represents display name of Crm entity.
        /// </summary>
        /// <param name="displayName">JSON to be used to load display name.</param>
        /// <param name="localizedLabelElementName">Element in json which contains labels for each locale.</param>
		public DisplayName(JObject displayName, string localizedLabelElementName = "LocalizedLabels")
        {
            this.LocalizedLabels = ((displayName[localizedLabelElementName] as JArray) ?? new JArray())
                .Select(localizedLabel => new LocalizedLabel(localizedLabel as JObject))
                .ToList();
        }

        /// <summary>
        /// Represents display name of Crm entity.
        /// </summary>
        /// <param name="displayName">Collection of XML to load displayname</param>
        public DisplayName(IEnumerable<XElement> displayName)
        {
            this.LocalizedLabels = displayName.Select(label => new LocalizedLabel(label));
        }
    }
}
