namespace Words
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class WordFinder
    {
        private readonly ITree tree;
        private readonly Dictionary<string, SortedSet<string>> permutations;
        private readonly Dictionary<string, string> normalizedToOriginal;
        private readonly Language language;

        private WordFinder(ITree tree,
            Dictionary<string, SortedSet<string>> permutations,
            Dictionary<string, string> normalizedToOriginal,
            Language language,
            string treeType)
        {
            this.tree = tree;
            this.permutations = permutations;
            this.normalizedToOriginal = normalizedToOriginal;
            this.language = language;
            TreeType = treeType;
        }

        public string TreeType { get; }

        public static WordFinder CreateTernary(string[] lines, Language language)
        {
            var tree = new TernaryTree(language);
            var permutations = new Dictionary<string, SortedSet<string>>();
            var normalizedToOriginal = new Dictionary<string, string>();
            var words = Randomize(lines);
            foreach (var word in words)
            {
                var normalized = language.ToLower(word);

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

                tree.Add(normalized);

                // sort characters and use that as key
                var chars = normalized.ToCharArray();
                Array.Sort(chars);
                var key = new string(chars);
                if (permutations.TryGetValue(key, out var list))
                    list.Add(word);
                else
                    permutations.Add(key, new SortedSet<string> { word });
            }

            return new WordFinder(tree, permutations, normalizedToOriginal, language, "TERN");
        }

        public static WordFinder CreateSuccinct(
            string[] lines,
            SuccinctTreeData succinctTreeData,
            Language language)
        {
            var tree = new SuccinctTree(succinctTreeData, language);
            var permutations = new Dictionary<string, SortedSet<string>>();
            var normalizedToOriginal = new Dictionary<string, string>();
            foreach (var word in lines)
            {
                var normalized = language.ToLower(word);

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

                // sort characters and use that as key
                var chars = normalized.ToCharArray();
                Array.Sort(chars);
                var key = new string(chars);
                if (permutations.TryGetValue(key, out var list))
                    list.Add(word);
                else
                    permutations.Add(key, new SortedSet<string> { word });
            }

            return new WordFinder(tree, permutations, normalizedToOriginal, language, "SUCC");
        }

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
            var normalized = language.ToLower(input);
            foreach (var s in tree.Matches(normalized, limit).Select(m => new Match { Value = normalizedToOriginal[m], Type = MatchType.Word }))
                action.Invoke(s);
            foreach (var s in Anagram(input))
                action.Invoke(s);
            foreach (var s in Near(input, d))
                action.Invoke(s);
        }

        private List<Match> Anagram(string input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            var normalized = language.ToLower(input);
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
                        list.Where(m => language.ToLower(m) != normalized)
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
            var normalized = language.ToLower(input);
            var matches = tree.NearSearch(normalized, d)
                .Where(m => m != normalized)
                .Select(m => new Match
                {
                    Value = normalizedToOriginal[m],
                    Type = MatchType.Near
                }).ToList();
            return matches;
        }

        private static IEnumerable<string> Randomize(string[] list)
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
