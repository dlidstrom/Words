namespace Words
{
    using System.Diagnostics;

    [DebuggerDisplay("Char = {Char} WordEnd = {WordEnd} Children = {Children} NodeIndex = {NodeIndex}")]
    public class SuccinctNode
    {
        public SuccinctNode(Node node, int children, int nodeIndex)
        {
            Char = node.Char;
            WordEnd = node.WordEnd;
            Children = children;
            NodeIndex = nodeIndex;
        }

        public char Char { get; }

        public bool WordEnd { get; }

        public int Children { get; }

        public int NodeIndex { get; }

        public override string ToString()
        {
            return $"{nameof(Char)} = {Char}, {nameof(WordEnd)} = {WordEnd}";
        }
    }
}