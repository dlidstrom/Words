namespace Words
{
    using System.Diagnostics;

    [DebuggerDisplay("Char = {Char} WordEnd = {WordEnd} NodeIndex = {NodeIndex}")]
    public class SuccinctNode
    {
        public SuccinctNode(char c, bool wordEnd, int nodeIndex)
        {
            Char = c;
            WordEnd = wordEnd;
            NodeIndex = nodeIndex;
        }

        public char Char { get; }

        public bool WordEnd { get; }

        public int NodeIndex { get; }

        public override string ToString()
        {
            return $"{nameof(Char)} = {Char}, {nameof(WordEnd)} = {WordEnd}";
        }
    }
}