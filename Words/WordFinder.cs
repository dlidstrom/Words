#nullable enable

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
            Dictionary<string, SortedSet<string>>? permutations,
            Func<string, string[]> getPermutations,
            Dictionary<string, string>? normalizedToOriginal,
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

        public ITree Advanced => tree;

        public Dictionary<string, SortedSet<string>>? Permutations { get; }

        public Dictionary<string, string>? NormalizedToOriginal { get; }

        public static WordFinder CreateTernary(string[] lines, Language language)
        {
            TernaryTree tree = new(language);
            Dictionary<string, SortedSet<string>> permutations = new();
            Dictionary<string, string> normalizedToOriginal = new();
            IEnumerable<string> words = lines.Randomize();
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

        public List<Match> Matches(string input, int d, SearchType searchType = SearchType.All, int limit = 100)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            List<Match> matches = new();
            Matches(input, matches.Add, d, searchType, limit);
            return matches;
        }

        private void Matches(
            string input,
            Action<Match> action,
            int d,
            SearchType searchType,
            int limit)
        {
            if ((searchType & SearchType.Word) != SearchType.None)
            {
                string normalized = language.ToLower(input);
                Match[] originalMatches = getOriginal.Invoke(tree.Matches(normalized, limit).ToArray())
                    .Select(m =>
                    {
                        Match match = new(m, MatchType.Word);
                        return match;
                    })
                    .ToArray();

                foreach (Match s in originalMatches)
                {
                    action.Invoke(s);
                }
            }

            if ((searchType & SearchType.Anagram) != SearchType.None)
            {
                foreach (Match s in Anagram(input))
                {
                    action.Invoke(s);
                }
            }

            if ((searchType & SearchType.Near) != SearchType.None)
            {
                foreach (Match s in Near(input, d))
                {
                    action.Invoke(s);
                }
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
                        .Select(m => new Match(m, MatchType.Anagram)));
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
                    Match match = new(m, MatchType.Near);
                    return match;
                })
                .ToArray();
            return matches;
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
