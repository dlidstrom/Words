namespace Words
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class WordFinder
    {
        private readonly ITree tree;
        private readonly Func<string, string[]> getPermutations;
        private readonly Func<string, string> getOriginal;
        private readonly Language language;

        private WordFinder(
            ITree tree,
            Dictionary<string, SortedSet<string>> permutations,
            Func<string, string[]> getPermutations,
            Dictionary<string, string> normalizedToOriginal,
            Func<string, string> getOriginal,
            Language language,
            string treeType)
        {
            this.tree = tree;
            this.getPermutations = getPermutations;
            this.getOriginal = getOriginal;
            this.language = language;
            Permutations = permutations;
            NormalizedToOriginal = normalizedToOriginal;
            TreeType = treeType;
        }

        public string TreeType { get; }

        public Dictionary<string, SortedSet<string>> Permutations { get; }

        public Dictionary<string, string> NormalizedToOriginal { get; }

        public static WordFinder CreateTernary(string[] lines, Language language)
        {
            TernaryTree tree = new TernaryTree(language);
            Dictionary<string, SortedSet<string>> permutations = new Dictionary<string, SortedSet<string>>();
            Dictionary<string, string> normalizedToOriginal = new Dictionary<string, string>();
            IEnumerable<string> words = Randomize(lines);
            foreach (string word in words)
            {
                string normalized = language.ToLower(word);

                // keep original
                if (normalizedToOriginal.TryGetValue(normalized, out string added) && added != word)
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

                tree.Add(normalized);

                // sort characters and use that as key
                char[] chars = normalized.ToCharArray();
                Array.Sort(chars);
                string key = new string(chars);
                if (permutations.TryGetValue(key, out SortedSet<string> list))
                    list.Add(word);
                else
                    permutations.Add(key, new SortedSet<string> { word });
            }

            WordFinder wordFinder = new WordFinder(
                tree,
                permutations,
                s =>
                {
                    if (permutations.TryGetValue(s, out SortedSet<string> list))
                    {
                        return list.ToArray();
                    }

                    return new string[0];
                },
                normalizedToOriginal,
                s => normalizedToOriginal[s],
                language,
                "TERN");
            return wordFinder;
        }

        public static WordFinder CreateSuccinct(
            SuccinctTreeData succinctTreeData,
            Language language,
            Func<string, string[]> getPermutations,
            Func<string, string> getOriginal)
        {
            SuccinctTree tree = new SuccinctTree(succinctTreeData, language);
            WordFinder wordFinder = new WordFinder(
                tree,
                null,
                getPermutations,
                null,
                getOriginal,
                language,
                "SUCC");
            return wordFinder;
        }

        public List<Match> Matches(string input, int d, int limit = 100)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            List<Match> matches = new List<Match>();
            Matches(input, matches.Add, d, limit);
            return matches;
        }

        private void Matches(string input, Action<Match> action, int d, int limit = 100)
        {
            string normalized = language.ToLower(input);
            Match[] originalMatches = tree.Matches(normalized, limit)
                .Select(m =>
                {
                    string original = getOriginal.Invoke(m) ?? m;
                    Match match = new Match
                    {
                        Value = original,
                        Type = MatchType.Word
                    };
                    return match;
                })
                .ToArray();
            foreach (Match s in originalMatches)
            {
                action.Invoke(s);
            }

            foreach (Match s in Anagram(input))
            {
                action.Invoke(s);
            }

            foreach (Match s in Near(input, d))
            {
                action.Invoke(s);
            }
        }

        private List<Match> Anagram(string input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            string normalized = language.ToLower(input);
            List<Match> matches = new List<Match>();
            if (input.IndexOfAny(new[] { '?', '@', '#', '*' }) < 0)
            {
                // also try to find permutations
                char[] chars = normalized.ToCharArray();
                Array.Sort(chars);
                string key = new string(chars);
                string[] list = getPermutations.Invoke(key);
                matches.AddRange(
                    list.Where(m => language.ToLower(m) != normalized)
                        .Select(m => new Match
                        {
                            Value = m,
                            Type = MatchType.Anagram
                        }));
            }

            return matches;
        }

        private Match[] Near(string input, int d)
        {
            string normalized = language.ToLower(input);
            Match[] matches = tree.NearSearch(normalized, d)
                .Where(m => m != normalized)
                .Select(m =>
                {
                    string original = getOriginal.Invoke(m) ?? m;
                    Match match = new Match
                    {
                        Value = original,
                        Type = MatchType.Near
                    };
                    return match;
                })
                .ToArray();
            return matches;
        }

        private static IEnumerable<string> Randomize(string[] list)
        {
            Random random = new Random();
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

        public SuccinctTree EncodeSuccinct()
        {
            if (tree is TernaryTree ternary)
            {
                return ternary.EncodeSuccinct();
            }

            return (SuccinctTree)tree;
        }
    }
}
