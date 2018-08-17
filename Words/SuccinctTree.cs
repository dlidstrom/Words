namespace Words
{
    using System;

    public class SuccinctTree
    {
        private readonly BitString encoding;
        private readonly SuccinctNode[] nodes;
        private readonly RankDirectory directory;

        public SuccinctTree(string encoding, SuccinctNode[] nodes)
        {
            this.encoding = new BitString(encoding);
            directory = RankDirectory.Create(encoding, 2 * nodes.Length + 1, 32 * 32, 32);
            this.nodes = nodes;
        }

        public bool Contains(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                throw new ArgumentException();

            int nodeIndex = 0;
            int pos = 0;
            SuccinctNode node = nodes[0];
            while (node != null)
            {
                SuccinctNode left = null;
                SuccinctNode right = null;
                SuccinctNode center = null;

                LoadChildren(nodeIndex, ref center, ref left, ref right);
                if (s[pos] < node.Char)
                    node = left;
                else if (s[pos] > node.Char)
                    node = right;
                else
                {
                    if (++pos == s.Length)
                        return node.WordEnd;
                    node = center;
                }
            }

            return false;
        }

        private void LoadChildren(
            int nodeIndex,
            ref SuccinctNode center,
            ref SuccinctNode left,
            ref SuccinctNode right)
        {
            // figure out which is which
            // center is always first child
            // left is < center
            // right is > center
            var numberOfChildren = 0;
            switch (numberOfChildren)
            {
                case 3:
                {
                    // load nodes 1-3 from nodeIndex
                    // select0(i + 1) - i
                    break;
                }

                case 2:
                {
                    break;
                }

                case 1:
                {
                    break;
                }
            }
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