#nullable enable

namespace Words
{
    using System.Diagnostics;

    [DebuggerDisplay("Character = {Character} WordEnd = {WordEnd} NodeIndex = {NodeIndex}")]
    public class SuccinctNode
    {
        public SuccinctNode(char c, int nodeIndex)
        {
            Character = c;
            NodeIndex = nodeIndex;
        }

        public char Character { get; }

        public bool WordEnd => Character == 0;

        public int NodeIndex { get; }

        public override string ToString()
        {
            return $"{nameof(Character)} = {Character}, {nameof(WordEnd)} = {WordEnd}";
        }
    }
}