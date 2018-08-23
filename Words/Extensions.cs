namespace Words
{
    public static class Extensions
    {
        public static SuccinctNode GetNode(this BitString bitString, int nodeIndex)
        {
            var data = bitString.Get(nodeIndex * (16 + 1), 16 + 1);
            var wordEnd = data >> 16 == 1;
            var c = (char)(data & 0xFFFF);
            return new SuccinctNode(c, wordEnd, nodeIndex);
        }

        public static void Write(this BitWriter bitWriter, Node node)
        {
            bitWriter.Write(node.WordEnd ? 1 : 0, 1);
            bitWriter.Write(node.Char, 16);
        }
    }
}