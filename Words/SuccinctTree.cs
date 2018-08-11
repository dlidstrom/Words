namespace Words
{
    public class SuccinctTree
    {
        private readonly string encoding;
        private readonly SuccinctNode[] nodes;

        public SuccinctTree(string encoding, SuccinctNode[] nodes)
        {
            this.encoding = encoding;
            this.nodes = nodes;
        }

        public void Traverse()
        {
            var nodeIndex = 0;
            var node = nodes[nodeIndex];

            // load children
            var childNodeIndex = Select(nodeIndex + 1) - nodeIndex;
        }

        public int Select(int index)
        {
            var pos = 0;
            while (true)
            {
                if (Rank(pos) == index)
                    return pos;

                pos++;
            }
        }

        public int Rank(int position)
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