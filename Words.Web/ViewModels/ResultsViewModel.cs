#nullable enable

namespace Words.Web.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class ResultsViewModel
    {
        public ResultsViewModel(string query, IList<Match> matches, double elapsedMilliseconds)
        {
            if (matches == null)
            {
                throw new ArgumentNullException(nameof(matches));
            }

            Query = query;
            Words = matches.Where(m => m.Type == MatchType.Word).Select(m => m.Value).ToList();
            Anagrams = matches.Where(m => m.Type == MatchType.Anagram).Select(m => m.Value).ToList();
            Near = matches.Where(m => m.Type == MatchType.Near).Select(m => m.Value).ToList();
            ElapsedMilliseconds = elapsedMilliseconds;
        }

        public string Query { get; }
        public int Count => Words.Count + Anagrams.Count + Near.Count;
        public double ElapsedMilliseconds { get; }
        public List<string> Words { get; }
        public List<string> Anagrams { get; }
        public List<string> Near { get; }
    }
}