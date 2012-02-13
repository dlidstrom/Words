namespace Words
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;

    public class WordFinder
    {
        private readonly Dictionary<string, SortedSet<string>> permutations = new Dictionary<string, SortedSet<string>>();
        private readonly Dictionary<string, string> normalizedToOriginal = new Dictionary<string, string>();
        private readonly CultureInfo cultureInfo;

        public WordFinder(string filename, Encoding encoding, Language language)
        {
            Tree = new TernaryTree(language);
            this.cultureInfo = language.CultureInfo;
            var lines = File.ReadAllLines(filename, encoding);
            foreach (var word in Randomize(lines))
            {
                string normalized = word.ToLower(cultureInfo);
                Tree.Add(normalized);

                // keep original
                string added;
                if (normalizedToOriginal.TryGetValue(normalized, out added) && added != word)
                {
                    throw new Exception(string.Format("Two words normalize to the same value: {0} and {1} -> {2}", word, added, normalized));
                }
                else
                {
                    normalizedToOriginal.Add(normalized, word);
                }

                // sort characters and use that as key
                var chars = normalized.ToCharArray();
                Array.Sort(chars);
                string key = new string(chars);
                SortedSet<string> list;
                if (permutations.TryGetValue(key, out list))
                    list.Add(word);
                else
                    permutations.Add(key, new SortedSet<string> { word });
            }
        }

        public TernaryTree Tree
        {
            get;
            private set;
        }

        public List<Match> Matches(string input)
        {
            string normalized = input.ToLower(cultureInfo);
            var matches = Tree.Matches(normalized)
                .Select(m => new Match { Value = normalizedToOriginal[m], Type = MatchType.Word })
                .ToList();
            matches.AddRange(Anagram(input));
            matches.AddRange(Near(input));

            return matches;
        }

        public List<Match> Anagram(string input)
        {
            string normalized = input.ToLower(cultureInfo);
            List<Match> matches = new List<Match>();
            if (input.IndexOfAny(new char[] { '?', '@', '#', '*' }) < 0)
            {
                // also try to find permutations
                var chars = normalized.ToCharArray();
                Array.Sort(chars);
                string key = new string(chars);
                SortedSet<string> list;
                if (permutations.TryGetValue(key, out list))
                {
                    matches.AddRange(
                        list.Where(m => m.ToLower(cultureInfo) != normalized)
                            .Select(m => new Match
                            {
                                Value = m,
                                Type = MatchType.Anagram
                            }));
                }
            }

            return matches;
        }

        public List<Match> Near(string input)
        {
            string normalized = input.ToLower(cultureInfo);
            return Tree.NearSearch(input)
                .Where(m => m != normalized)
                .Select(m => new Match
                {
                    Value = normalizedToOriginal[m],
                    Type = MatchType.Near
                }).ToList();
        }

        private IEnumerable<string> Randomize(string[] list)
        {
            var random = new Random();
            int n = list.Length;
            while (n > 0)
            {
                n--;
                int k = random.Next(n + 1);
                string value = list[k];
                list[k] = list[n];
                list[n] = value;
                yield return value;
            }
        }
    }
}
