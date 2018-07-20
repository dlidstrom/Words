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

        public WordFinder(
            string filename,
            Encoding encoding,
            Language language)
        {
            Tree = new TernaryTree(language);
            cultureInfo = language.CultureInfo;
            var lines = File.ReadAllLines(filename, encoding);
            var words = Randomize(lines);
            foreach (var word in words)
            {
                string normalized = word.ToLower(cultureInfo);

                // keep original
                if (normalizedToOriginal.TryGetValue(normalized, out var added) && added != word)
                {
                    throw new Exception($"Two words normalize to the same value: {word} and {added} -> {normalized}");
                }

                try
                {
                    normalizedToOriginal.Add(normalized, word);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Duplicate word found: {word}", ex);
                }

                Tree.Add(normalized);

                // sort characters and use that as key
                var chars = normalized.ToCharArray();
                Array.Sort(chars);
                var key = new string(chars);
                if (permutations.TryGetValue(key, out var list))
                    list.Add(word);
                else
                    permutations.Add(key, new SortedSet<string> { word });
            }
        }

        private TernaryTree Tree
        {
            get;
        }

        public int Nodes { get; private set; }

        public List<Match> Matches(string input, int d, int limit = 100)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            var matches = new List<Match>();
            Matches(input, matches.Add, d, limit);
            return matches;
        }

        private void Matches(string input, Action<Match> action, int d, int limit = 100)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            string normalized = input.ToLower(cultureInfo);
            foreach (var s in Tree.Matches(normalized, limit).Select(m => new Match { Value = normalizedToOriginal[m], Type = MatchType.Word }))
                action.Invoke(s);
            Nodes = Tree.Nodes;
            foreach (var s in Anagram(input))
                action.Invoke(s);
            Nodes += Tree.Nodes;
            foreach (var s in Near(input, d))
                action.Invoke(s);
            Nodes += Tree.Nodes;
        }

        private List<Match> Anagram(string input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            string normalized = input.ToLower(cultureInfo);
            var matches = new List<Match>();
            if (input.IndexOfAny(new[] { '?', '@', '#', '*' }) < 0)
            {
                // also try to find permutations
                var chars = normalized.ToCharArray();
                Array.Sort(chars);
                var key = new string(chars);
                if (permutations.TryGetValue(key, out var list))
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

        private List<Match> Near(string input, int d)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            string normalized = input.ToLower(cultureInfo);
            return Tree.NearSearch(normalized, d)
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
