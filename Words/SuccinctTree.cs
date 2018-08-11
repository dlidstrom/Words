namespace Words
{
    using System;

    public class SuccinctTree
    {
        private readonly string encoding;
        private readonly SuccinctNode[] nodes;

        public SuccinctTree(string encoding, SuccinctNode[] nodes)
        {
            this.encoding = encoding;
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

                // figure out which is which
                // center is always first child
                // left is < center
                // right is > center
                switch (node.Children)
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

        public int Select(int which, int index)
        {
            var pos = 0;
            while (true)
            {
                if (Rank(which, pos) == index)
                    return pos;

                pos++;
            }
        }

        public int Rank(int which, int position)
        {
            var rank = 0;
            for (var i = 0; i <= position; i++)
            {
                if (encoding[i] == '1') rank++;
            }

            return rank;
        }
    }
}