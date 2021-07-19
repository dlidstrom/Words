namespace Words
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class SuccinctTree : ITree
    {
        private readonly RankDirectory directory;
        private readonly Language language;
        private readonly BitString encoding;
        private readonly int encodingBits;
        private readonly BitString letterData;

        public SuccinctTree(
            (string data, int totalBits) encoding,
            (string data, int totalBits) letters,
            Language language)
        {
            this.encoding = new BitString(encoding.data);
            encodingBits = encoding.totalBits;
            letterData = new BitString(letters.data);
            directory = RankDirectory.Create(
                encoding,
                32 * 32,
                32);
            this.language = language;
        }

        public SuccinctTree(SuccinctTreeData data, Language language)
        {
            string encodingJoined = string.Join(string.Empty, data.EncodingBytes);
            encoding = new BitString(encodingJoined);
            encodingBits = data.EncodingBits;
            letterData = new BitString(string.Join(string.Empty, data.LetterBytes));
            directory = RankDirectory.Create(
                (encodingJoined, encodingBits),
                32 * 32,
                32);
            this.language = language;
        }

        public Action<string> Log { get; set; }

        public SuccinctTreeData GetData()
        {
            SuccinctTreeData data = new SuccinctTreeData(
                encoding.Bytes.ChunkSplit(80).ToArray(),
                encodingBits,
                letterData.Bytes.ChunkSplit(80).ToArray());
            return data;
        }

        public List<string> Matches(string s, int limit)
        {
            if (string.IsNullOrWhiteSpace(s))
                throw new ArgumentException();

            List<string> matches = new List<string>();

            const int pos = 0;
            SuccinctNode node = letterData.GetNode(0);
            Matches(s, string.Empty, pos, node, matches, limit);

            return matches;
        }

        public List<string> NearSearch(string s, int d, int limit)
        {
            if (string.IsNullOrWhiteSpace(s))
                throw new ArgumentException();

            List<string> matches = new List<string>();

            const int pos = 0;
            SuccinctNode node = letterData.GetNode(0);
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
            (SuccinctNode center, SuccinctNode left, SuccinctNode right) = LoadChildren(node);
            LogImpl(() => $"Visiting {node}, Left = {left?.Char}, Center = {center?.Char}, Right = {right?.Char}");

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

            (SuccinctNode center, SuccinctNode left, SuccinctNode right) = LoadChildren(node);
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
            SuccinctNode node)
        {
            int sel = directory.Select(node.NodeIndex + 1);
            int firstChild = sel - node.NodeIndex;
            int childOfNextNode = directory.Select(node.NodeIndex + 2, sel, sel + 8) - node.NodeIndex - 1;
            int numberOfChildren = childOfNextNode - firstChild;
            switch (numberOfChildren)
            {
                case 3:
                {
                        SuccinctNode center = letterData.GetNode(firstChild);
                        SuccinctNode left = letterData.GetNode(firstChild + 1);
                        SuccinctNode right = letterData.GetNode(firstChild + 2);
                    return (center, left, right);
                }

                case 2:
                {
                        SuccinctNode center = letterData.GetNode(firstChild);
                        SuccinctNode leftOrRight = letterData.GetNode(firstChild + 1);
                    if (leftOrRight.Char < node.Char)
                    {
                            SuccinctNode left = leftOrRight;
                        return (center, left, null);
                    }

                        SuccinctNode right = leftOrRight;
                    return (center, null, right);
                }

                case 1:
                {
                        SuccinctNode child = letterData.GetNode(firstChild);
                    if (node.WordEnd)
                    {
                        if (child.Char < node.Char)
                        {
                                SuccinctNode left = child;
                            return (null, left, null);
                        }

                        if (child.Char > node.Char)
                        {
                                SuccinctNode right = child;
                            return (null, null, right);
                        }
                    }

                        SuccinctNode center = child;
                    return (center, null, null);
                }

                default:
                {
                    return (null, null, null);
                }
            }
        }

        private void LogImpl(Func<string> func)
        {
            Log?.Invoke(func.Invoke());
        }
    }
}