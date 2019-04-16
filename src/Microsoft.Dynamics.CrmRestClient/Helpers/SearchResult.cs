namespace Microsoft.Dynamics.CrmRestClient
{
    using System.Collections.Generic;

    /// <summary>
    /// Search Result.
    /// </summary>
    public sealed class SearchResult
	{
        /// <summary>
        /// Defines what is a success result.
        /// </summary>
		internal const int SuccessResult = 0;

        /// <summary>
        /// Distance of search word with known keyword.
        /// </summary>
		public int Distance { get; private set; }

        /// <summary>
        /// String comparing against.
        /// </summary>
		public string ComparingString { get; private set; }

        /// <summary>
        /// Any object associated with the string.
        /// </summary>
		public object AssociatedValue { get; private set; }

        /// <summary>
        /// Is the search a success.
        /// </summary>
		public bool IsSuccess
		{
			get
			{
				return this.Distance.Equals(SearchResult.SuccessResult);
			}
		}

        /// <summary>
        /// Result of the search.
        /// </summary>
        /// <param name="distance">Distance of search word with known keyword.</param>
        /// <param name="comparingString">String comparing against.</param>
        /// <param name="associatedValue">Any object associated with the string.</param>
		internal SearchResult(int distance, string comparingString, object associatedValue)
		{
			this.Distance = distance;
			this.ComparingString = comparingString;
			this.AssociatedValue = associatedValue;
		}

        /// <summary>
        /// Get empty result set.
        /// </summary>
        /// <returns>Empty collection</returns>
		internal static IEnumerable<SearchResult> GetNoResults()
		{
			return new SearchResult[0];
		}

        /// <summary>
        /// Get single result as success
        /// </summary>
        /// <param name="comparingString">String comparing against.</param>
        /// <param name="associatedValue">Any object associated with the string.</param>
        /// <returns>A collection containing single <see cref="SearchResult"/> object</returns>
		internal static IEnumerable<SearchResult> GetSuccessResult(string comparingString, object associatedValue)
		{
			return new SearchResult[1] { new SearchResult(SearchResult.SuccessResult, comparingString, associatedValue) };
		}
	}
}
