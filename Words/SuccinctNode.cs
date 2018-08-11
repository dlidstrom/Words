namespace Words
{
    public class SuccinctNode
    {
        public SuccinctNode(Node node, int children)
        {
            Char = node.Char;
            WordEnd = node.WordEnd;
            Children = children;
        }

        public char Char { get; }

        public bool WordEnd { get; }

        public int Children { get; }
    }
}