namespace Words
{
    using System;

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

        public static int Select(this RankDirectory directory, int y)
        {
            return directory.Select(0, y, y - 1, Math.Min(directory.NumBits, 4 * y + 1));
        }

        public static int Select(this RankDirectory directory, int y, int low, int high)
        {
            return directory.Select(0, y, low, Math.Min(directory.NumBits, high));
        }
    }
}