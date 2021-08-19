﻿#nullable enable

namespace Words
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class WordFinder
    {
        private readonly ITree tree;
        private readonly Func<string, string[]> getPermutations;
        private readonly Func<string[], string[]> getOriginal;
        private readonly Language language;

        private WordFinder(
            ITree tree,
            Dictionary<string, SortedSet<string>> permutations,
            Func<string, string[]> getPermutations,
            Dictionary<string, string> normalizedToOriginal,
            Func<string[], string[]> getOriginal,
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
            TernaryTree tree = new(language);
            Dictionary<string, SortedSet<string>> permutations = new();
            Dictionary<string, string> normalizedToOriginal = new();
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
                string key = new(chars);
                if (permutations.TryGetValue(key, out SortedSet<string> list))
                {
                    _ = list.Add(word);
                }
                else
                {
                    permutations.Add(key, new SortedSet<string> { word });
                }
            }

            WordFinder wordFinder = new(
                tree,
                permutations,
                s =>
                {
                    return
                        permutations.TryGetValue(s, out SortedSet<string> list)
                        ? list.ToArray()
                        : (new string[0]);
                },
                normalizedToOriginal,
                s => s.Select(x => normalizedToOriginal[x]).ToArray(),
                language,
                "TERN");
            return wordFinder;
        }

        public static WordFinder CreateSuccinct(
            SuccinctTreeData succinctTreeData,
            Language language,
            Func<string, string[]> getPermutations,
            Func<string[], string[]> getOriginal)
        {
            SuccinctTree tree = new(succinctTreeData, language);
            WordFinder wordFinder = new(
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
            {
                throw new ArgumentNullException(nameof(input));
            }

            List<Match> matches = new();
            Matches(input, matches.Add, d, limit);
            return matches;
        }

        private void Matches(string input, Action<Match> action, int d, int limit = 100)
        {
            string normalized = language.ToLower(input);
            Match[] originalMatches = getOriginal.Invoke(tree.Matches(normalized, limit).ToArray())
                .Select(m =>
                {
                    Match match = new()
                    {
                        Value = m,
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
            string normalized = language.ToLower(input);
            List<Match> matches = new();
            if (input.IndexOfAny(new[] { '?', '@', '#', '*' }) < 0)
            {
                // also try to find permutations
                char[] chars = normalized.ToCharArray();
                Array.Sort(chars);
                string key = new(chars);
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
            string[] hits = tree.NearSearch(normalized, d).Where(m => m != normalized).ToArray();
            Match[] matches = getOriginal.Invoke(hits)
                .Select(m =>
                {
                    Match match = new()
                    {
                        Value = m,
                        Type = MatchType.Near
                    };
                    return match;
                })
                .ToArray();
            return matches;
        }

        private static IEnumerable<string> Randomize(string[] list)
        {
            Random random = new();
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

#pragma warning disable IDE0046 // Convert to conditional expression, fails here
            if (tree is SuccinctTree succinct)
#pragma warning restore IDE0046
            {
                return succinct;
            }

            throw new Exception($"Unrecognized tree {tree.GetType()}");
        }
    }
}
