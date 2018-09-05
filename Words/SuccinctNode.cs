namespace Words
{
    using System.Diagnostics;

    [DebuggerDisplay("Char = {Char} WordEnd = {WordEnd} NodeIndex = {NodeIndex}")]
    public class SuccinctNode
    {
        public SuccinctNode(char c, int nodeIndex)
        {
            Char = c;
            NodeIndex = nodeIndex;
        }

        public char Char { get; }

        public bool WordEnd => Char == 0;

        public int NodeIndex { get; }

        public override string ToString()
        {
            return $"{nameof(Char)} = {Char}, {nameof(WordEnd)} = {WordEnd}";
        }
    }
}