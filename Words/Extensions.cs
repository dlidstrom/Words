namespace Words
{
    using System;
    using System.Collections.Generic;

    public static class Extensions
    {
        public static SuccinctNode GetNode(this BitString bitString, int nodeIndex)
        {
            int data = bitString.Get(nodeIndex * 16, 16);
            char c = (char)data;
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

        public static IEnumerable<string> ChunkSplit(this string str, int chunkSize)
        {
            for (int i = 0; i < str.Length; i += chunkSize)
                yield return str.Substring(i, Math.Min(chunkSize, str.Length - i));
        }
    }
}