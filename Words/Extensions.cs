namespace Words
{
    public static class Extensions
    {
        public static SuccinctNode GetNode(this BitString bitString, int nodeIndex)
        {
            var data = bitString.Get(nodeIndex * 16, 16);
            var c = (char)data;
            return new SuccinctNode(c, nodeIndex);
        }

        public static void Write(this BitWriter bitWriter, Node node)
        {
            bitWriter.Write(node.Char, 16);
        }
    }
}