namespace Words
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    public class SuccinctTree
    {
        private readonly SuccinctNode[] nodes;
        private readonly RankDirectory directory;
        private readonly Language language;

        public SuccinctTree(string encoding, SuccinctNode[] nodes, Language language)
        {
            Encoding = new BitString(encoding);
            directory = RankDirectory.Create(encoding, 2 * nodes.Length + 1, 32 * 32, 32);
            this.nodes = nodes;
            this.language = language;
        }

        public BitString Encoding { get; }

        public ReadOnlyCollection<SuccinctNode> Nodes => new ReadOnlyCollection<SuccinctNode>(nodes);

        public bool Contains(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                throw new ArgumentException();

            int pos = 0;
            SuccinctNode node = nodes[0];
            while (node != null)
            {
                (var center, var left, var right) = LoadChildren(node.NodeIndex);
                if (s[pos] < node.Char)
                    node = left;
                else if (s[pos] > node.Char)
                    node = right;
                else
                {
                    if (++pos >= s.Length)
                        return node.WordEnd;
                    node = center;
                }
            }

            return false;
        }

        public List<string> Matches(string s, int limit = 100)
        {
            if (string.IsNullOrWhiteSpace(s))
                throw new ArgumentException();

            var matches = new List<string>();

            const int pos = 0;
            var node = nodes[0];
            //Nodes = 0;
            Matches(s, string.Empty, pos, node, matches, limit);

            return matches;
        }

        private (int x, int y) Xxx()
        {
            return (1, 2);
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
            //Nodes++;
            (var center, var left, var right) = LoadChildren(node.NodeIndex);

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
                    var center = nodes[firstChild];
                    var left = nodes[firstChild + 1];
                    var right = nodes[firstChild + 2];
                    return (center, left, right);
                }

                case 2:
                {
                    var center = nodes[firstChild];
                    var leftOrRight = nodes[firstChild + 1];
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
                    var center = nodes[firstChild];
                    return (center, null, null);
                }
            }

            throw new ApplicationException($"Unexpected number of children: {numberOfChildren}");
        }

        public int Select(int which, int index)
        {
            var select = directory.Select(which, index);
            return select;
        }

        public int Rank(int which, int position)
        {
            var rank = directory.Rank(which, position);
            return rank;
        }
    }
}