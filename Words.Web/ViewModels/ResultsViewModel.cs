namespace Words.Web.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;

    public class ResultsViewModel
    {
        public ResultsViewModel(IEnumerable<Match> matches, double elapsedMilliseconds)
        {
            Words = matches.Where(m => m.Type == MatchType.Word).Select(m => m.Value).ToList();
            Anagrams = matches.Where(m => m.Type == MatchType.Anagram).Select(m => m.Value).ToList();
            Near = matches.Where(m => m.Type == MatchType.Near).Select(m => m.Value).ToList();
            ElapsedMilliseconds = elapsedMilliseconds;
        }

        public int Count { get { return Words.Count + Anagrams.Count + Near.Count; } }
        public double ElapsedMilliseconds { get; set; }
        public List<string> Words { get; private set; }
        public List<string> Anagrams { get; private set; }
        public List<string> Near { get; private set; }
    }
}