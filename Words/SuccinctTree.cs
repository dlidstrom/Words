namespace Words
{
    using System;
    using System.Collections.Generic;

    public class SuccinctTree : ITree
    {
        private readonly RankDirectory directory;
        private readonly Language language;

        public SuccinctTree(
            (string data, int totalBits) encoding,
            (string data, int totalBits) letters,
            Language language)
        {
            Encoding = new BitString(encoding.data);
            EncodingBits = encoding.totalBits;
            LetterData = new BitString(letters.data);
            directory = RankDirectory.Create(
                encoding,
                32 * 32,
                32);
            this.language = language;
        }

        public SuccinctTree(SuccinctTreeData data, Language language)
        {
            Encoding = new BitString(data.EncodingBytes);
            EncodingBits = data.EncodingBits;
            LetterData = new BitString(data.LetterBytes);
            directory = RankDirectory.Create(
                (data.EncodingBytes, data.EncodingBits),
                32 * 32,
                32);
            this.language = language;
        }

        public BitString Encoding { get; }

        public int EncodingBits { get; }

        public BitString LetterData { get; }

        public Action<string> Log { get; set; } = s => { };

        public SuccinctTreeData GetData()
        {
            return new SuccinctTreeData(Encoding.Bytes, EncodingBits, LetterData.Bytes);
        }

        public List<string> Matches(string s, int limit)
        {
            if (string.IsNullOrWhiteSpace(s))
                throw new ArgumentException();

            var matches = new List<string>();

            const int pos = 0;
            var node = LetterData.GetNode(0);
            Matches(s, string.Empty, pos, node, matches, limit);

            return matches;
        }

        public List<string> NearSearch(string s, int d, int limit)
        {
            if (string.IsNullOrWhiteSpace(s))
                throw new ArgumentException();

            var matches = new List<string>();

            const int pos = 0;
            var node = LetterData.GetNode(0);
            NearSearch(s, string.Empty, pos, node, matches, limit, d);

            return matches;
        }

        private void Matches(
            string s,
            string substr,
            int pos,
            SuccinctNode node,
            List<string> matches,
            int limit)
        {
            if (node == null || matches.Count >= limit)
                return;
            (var center, var left, var right) = LoadChildren(node.NodeIndex);
            Log($"Visiting {node}, Left = {left?.Char}, Center = {center?.Char}, Right = {right?.Char}");

            char c = pos == s.Length ? default(char) : s[pos];
            if (WildcardMatchLeft(c, node.Char) || c < node.Char)
                Matches(s, substr, pos, left, matches, limit);

            if (WildcardMatch(c, node.Char) || c == node.Char)
                Matches(s, substr + node.Char, pos + 1, center, matches, limit);

            if (c == default(char) && node.WordEnd)
                matches.Add(substr);

            if (WildcardMatchRight(c, node.Char) || c > node.Char)
                Matches(s, substr, pos, right, matches, limit);
        }

        private bool WildcardMatch(char c, char node)
        {
            if (c == '?')
                return true;
            if (c == '#')
                return language.Consonants.Contains(node);
            if (c == '@')
                return language.Vowels.Contains(node);
            return false;
        }

        private bool WildcardMatchLeft(char c, char node)
        {
            if (c == '?')
                return true;
            if (c == '#')
                return language.Consonants.Min <= node;
            if (c == '@')
                return language.Vowels.Min <= node;
            return false;
        }

        private bool WildcardMatchRight(char c, char node)
        {
            if (c == '?')
                return true;
            if (c == '#')
                return node <= language.Consonants.Max;
            if (c == '@')
                return node <= language.Vowels.Max;
            return false;
        }

        private void NearSearch(string s, string substr, int pos, SuccinctNode node, List<string> matches, int limit, int depth)
        {
            if (node == null || matches.Count >= limit || depth < 0)
                return;

            (var center, var left, var right) = LoadChildren(node.NodeIndex);
            char c = default(char);
            if (pos < s.Length)
                c = s[pos];

            if (depth > 0 || c < node.Char)
                NearSearch(s, substr, pos, left, matches, limit, depth);

            if (node.WordEnd)
            {
                if (s.Length - pos <= depth)
                    matches.Add(substr);
            }
            else
            {
                int newDepth = c == node.Char ? depth : depth - 1;
                if (c != default(char))
                    NearSearch(s, substr + node.Char, pos + 1, center, matches, limit, newDepth);
                else
                    NearSearch(s, substr + node.Char, pos, center, matches, limit, newDepth);
            }

            if (depth > 0 || c > node.Char)
                NearSearch(s, substr, pos, right, matches, limit, depth);
        }

        private (SuccinctNode center, SuccinctNode left, SuccinctNode right) LoadChildren(
            int nodeIndex)
        {
            // figure out which is which
            // center is always first child
            // left is < center
            // right is > center
            var firstChild = directory.Select(0, nodeIndex + 1) - nodeIndex;
            var childOfNextNode = directory.Select(0, nodeIndex + 2) - nodeIndex - 1;
            var numberOfChildren = childOfNextNode - firstChild;
            switch (numberOfChildren)
            {
                case 3:
                {
                    var center = LetterData.GetNode(firstChild);
                    var left = LetterData.GetNode(firstChild + 1);
                    var right = LetterData.GetNode(firstChild + 2);
                    return (center, left, right);
                }

                case 2:
                {
                    var center = LetterData.GetNode(firstChild);
                    var leftOrRight = LetterData.GetNode(firstChild + 1);
                    if (leftOrRight.Char < center.Char)
                    {
                        var left = leftOrRight;
                        return (center, left, null);
                    }

                    var right = leftOrRight;
                    return (center, null, right);
                }

                case 1:
                {
                    var center = LetterData.GetNode(firstChild);
                    return (center, null, null);
                }

                default:
                {
                    return (null, null, null);
                }
            }
        }
    }
}