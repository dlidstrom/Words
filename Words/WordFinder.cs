﻿namespace Words
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

            var wordFinder = new WordFinder(
                tree,
                permutations,
                s =>
                {
                    if (permutations.TryGetValue(s, out var list))
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
            var tree = new SuccinctTree(succinctTreeData, language);
            var wordFinder = new WordFinder(
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

            var matches = new List<Match>();
            Matches(input, matches.Add, d, limit);
            return matches;
        }

        private void Matches(string input, Action<Match> action, int d, int limit = 100)
        {
            var normalized = language.ToLower(input);
            var originalMatches = tree.Matches(normalized, limit)
                .Select(m =>
                {
                    var original = getOriginal.Invoke(m) ?? m;
                    var match = new Match
                    {
                        Value = original,
                        Type = MatchType.Word
                    };
                    return match;
                })
                .ToArray();
            foreach (var s in originalMatches)
            {
                action.Invoke(s);
            }

            foreach (var s in Anagram(input))
            {
                action.Invoke(s);
            }

            foreach (var s in Near(input, d))
            {
                action.Invoke(s);
            }
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
                var list = getPermutations.Invoke(key);
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
            var normalized = language.ToLower(input);
            var matches = tree.NearSearch(normalized, d)
                .Where(m => m != normalized)
                .Select(m =>
                {
                    var original = getOriginal.Invoke(m) ?? m;
                    var match = new Match
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
