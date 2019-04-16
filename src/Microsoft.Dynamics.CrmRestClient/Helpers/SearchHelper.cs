namespace Microsoft.Dynamics.CrmRestClient
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Class uses fuzzy search to match given string in a collection of strings.
    /// </summary>
    [Serializable]
	public static class SearchHelper
	{
        /// <summary>
        /// Defines the default number of results to be returned.
        /// </summary>
		public const int DefaultResultsToReturn = 3;

		/// <summary>
		/// Gets edit distance between two strings. Uses the Wagner–Fischer algorithm to get the edit distance. 
		/// Edit distance also known as Levenshtein distance computes how similar two strings are by either deleting, adding, or substituting letters. 
		/// </summary>
		/// <param name="inputString">Input string that will be compared against known string</param>
		/// <param name="comparisonString"> string to be compared against</param>
		/// <returns>Edit Distance</returns>
		private static int GetEditDistance(string inputString, string comparisonString)
		{
			int[,] distance = new int[inputString.Length, comparisonString.Length];
			
			for (int inputStringIndex = 0; inputStringIndex < inputString.Length; inputStringIndex++)
			{
				distance[inputStringIndex, 0] = inputStringIndex;
			}
			for (int comparisonStringIndex = 0; comparisonStringIndex < comparisonString.Length; comparisonStringIndex++)
			{
				distance[0, comparisonStringIndex] = comparisonStringIndex;
			}

			for (int inputStringIndex = 1; inputStringIndex < inputString.Length; inputStringIndex++)
			{
				for (int comparisonStringIndex = 1; comparisonStringIndex < comparisonString.Length; comparisonStringIndex++)
				{
					// Letters are the same, no need to edit
					if (inputString[inputStringIndex].Equals(comparisonString[comparisonStringIndex]))
					{
						distance[inputStringIndex, comparisonStringIndex] = distance[inputStringIndex - 1, comparisonStringIndex - 1];
					}
					else
					{
						// New distance is going to be the min of making the actions. 
						// First option is delete a letter, second is add, third is substitute a letter. 
						distance[inputStringIndex, comparisonStringIndex] =
							Math.Min(
								distance[inputStringIndex - 1, comparisonStringIndex] + 1,
								Math.Min(
									distance[inputStringIndex, comparisonStringIndex - 1] + 1,
									distance[inputStringIndex - 1, comparisonStringIndex - 1] + 2));
					}
				}
			}

			return distance[inputString.Length - 1, comparisonString.Length - 1];
		}

		/// <summary>
		/// Search list of words for closest matches from input word.
		/// </summary>
		/// <param name="searchWord">single string that needs to be searched</param>
		/// <param name="knownWords">List of strings to be compared against</param>
		/// <param name="resultsToReturn">number of results to be returned</param>
		/// <returns>List of tuples of ints, string, object. In order: Similarity (edit) distance, ascending scores, string that was compared, and object the comparison string came from </returns>
		public static IEnumerable<SearchResult> Search(string searchWord, IEnumerable<KeyValuePair<string, object>> knownWords, int resultsToReturn = SearchHelper.DefaultResultsToReturn)
		{
			List<SearchResult> mostSimilarWords = new List<SearchResult>();

			// Search Word Sanitation
			// Regex makes all multiple spaces become one space. Example: "account      name" -> "account name"
			// Trim removes leading and trailing zeros
			searchWord = Regex.Replace(searchWord.ToLower(), @"\s+", " ").Trim();

			knownWords = knownWords.Where(word => !string.IsNullOrEmpty(word.Key)).ToList();

			// Check for perfect match before running algorithm
			if (knownWords.Any(pair => pair.Key.Equals(searchWord, StringComparison.InvariantCultureIgnoreCase)))
			{
				mostSimilarWords.Add(
					knownWords.Where(pair => pair.Key.Equals(searchWord, StringComparison.InvariantCultureIgnoreCase)).Select(pair => new SearchResult(0, pair.Key, pair.Value)).FirstOrDefault());
				return mostSimilarWords;
			}

			foreach (KeyValuePair<string, object> word in knownWords)
			{
				string comparisonWord = word.Key.ToLowerInvariant();

				// Get distance between this string and the inputted string
				int editDistance = SearchHelper.GetEditDistance(searchWord, comparisonWord);

				// If there are less than # items wanted to be returned then must add and sort the top list 
				if (mostSimilarWords.Count < resultsToReturn)
				{
					mostSimilarWords.Add(new SearchResult(editDistance, word.Key, word.Value));
					mostSimilarWords.Sort((prior, posterior) => prior.Distance.CompareTo(posterior.Distance));
				}

				// If new distance is shorter, then replace it on list
				else if (editDistance < mostSimilarWords.Last().Distance)
				{
					mostSimilarWords[mostSimilarWords.Count - 1] = new SearchResult(editDistance, word.Key, word.Value);

					// Do neccessary swaps to get the word in the right place
					mostSimilarWords.Sort((prior, posterior) => prior.Distance.CompareTo(posterior.Distance));
				}
			}
			return mostSimilarWords;
		}
	}
}
