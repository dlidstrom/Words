namespace Words
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;

    public class NianFinder
    {
        private readonly Dictionary<string, SortedSet<string>> permutations = new Dictionary<string, SortedSet<string>>();
        private readonly CultureInfo cultureInfo;

        public NianFinder(string nianWordFile, Encoding encoding, CultureInfo cultureInfo)
        {
            this.cultureInfo = cultureInfo;
            var lines = File.ReadAllLines(nianWordFile, encoding);
            foreach (var word in lines)
            {
                // sort characters and use that as key
                var chars = word.ToCharArray();
                Array.Sort(chars);
                var key = new string(chars);
                if (permutations.TryGetValue(key, out var list))
                    list.Add(word);
                else
                    permutations.Add(key, new SortedSet<string> { word });
            }
        }

        public List<Match> Nine(string input)
        {
            if (input.Length != 9)
                throw new ArgumentException("Invalid length (must be 9 characters)", nameof(input));

            SortedSet<string> combinations = Combinations(input);
            int e = input.Count(c => c == 'e');
            for (int i = 0; i < e; i++)
            {
                int ix = input.IndexOf('e');
                input = input.Substring(0, ix) + '�' + input.Substring(ix + 1);

                combinations.UnionWith(Combinations(input));
            }

            var matches = new List<Match>();
            foreach (var combination in combinations)
            {
                if (permutations.TryGetValue(combination, out var list))
                {
                    var anagrams = list.Select(m => new Match { Value = m, Type = MatchType.Anagram });
                    matches.AddRange(anagrams);
                }
            }

            matches.Sort((x, y) => cultureInfo.CompareInfo.Compare(x.Value, y.Value));
            return matches;
        }

        private static SortedSet<string> Combinations(string arr)
        {
            var result = new SortedSet<string>();
            char middle = arr[4];
            arr = arr.Substring(0, 4) + arr.Substring(5, 4);
            for (int i = 0; i < arr.Length; i++)
            {
                Combine(arr, new string(new[] { middle, arr[i] }), i + 1, ref result);
            }

            return result;
        }

        private static void Combine(string arr, string sub, int p, ref SortedSet<string> result)
        {
            if (sub.Length >= 4)
            {
                var chars = sub.ToCharArray();
                Array.Sort(chars);
                result.Add(new string(chars));
            }

            for (int i = p; i < arr.Length; i++)
                Combine(arr, sub + arr[i], i + 1, ref result);
        }
    }
}