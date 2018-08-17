namespace Words
{
    using System;

    public class RankDirectory
    {
        private readonly BitString directory;
        private readonly BitString data;
        private readonly int numBits;
        private readonly int l1Size;
        private readonly int l2Size;
        private readonly int l1Bits;
        private readonly int l2Bits;
        private readonly int sectionBits;

        private RankDirectory(string directoryData,
            string bitData,
            int numBits,
            int l1Size,
            int l2Size,
            int l1Bits,
            int l2Bits)
        {
            directory = new BitString(directoryData);
            data = new BitString(bitData);
            this.numBits = numBits;
            this.l1Size = l1Size;
            this.l2Size = l2Size;
            this.l1Bits = l1Bits;
            this.l2Bits = l2Bits;
            sectionBits = (l1Size / l2Size - 1) * l2Bits + l1Bits;
        }

        public static RankDirectory Create(
            string data, int numBits, int l1Size, int l2Size)
        {
            var bits = new BitString(data);
            var p = 0;
            var i = 0;
            var count1 = 0;
            var count2 = 0;
            var l1Bits = (int)Math.Ceiling(Math.Log(numBits) / Math.Log(2));
            var l2Bits = (int)Math.Ceiling(Math.Log(l1Size) / Math.Log(2));

            var directory = new BitWriter();

            while (p + l2Size <= numBits)
            {
                count2 += bits.Count(p, l2Size);
                i += l2Size;
                p += l2Size;
                if (i == l1Size)
                {
                    count1 += count2;
                    directory.Write(count1, l1Bits);
                    count2 = 0;
                    i = 0;
                }
                else
                {
                    directory.Write(count2, l2Bits);
                }
            }

            return new RankDirectory(
                directory.GetData().data,
                data,
                numBits,
                l1Size,
                l2Size,
                l1Bits,
                l2Bits);
        }

        public int Rank(int which, int x)
        {
            if (which == 0)
            {
                return x - Rank(1, x) + 1;
            }

            var rank = 0;
            var o = x;
            var sectionPos = 0;

            if (o >= l1Size)
            {
                sectionPos = o / l1Size * sectionBits;
                rank = directory.Get(sectionPos - l1Bits, l1Bits);
                o = o % l1Size;
            }

            if (o >= l2Size)
            {
                sectionPos += o / l2Size * l2Bits;
                rank += directory.Get(sectionPos - l2Bits, l2Bits);
            }

            rank += data.Count(x - x % l2Size, x % l2Size + 1);

            return rank;
        }

        public int Select(int which, int y)
        {
            var high = numBits;
            var low = -1;
            var val = -1;

            while (high - low > 1)
            {
                var probe = (high + low) / 2;
                var r = Rank(which, probe);

                if (r == y)
                {
                    // We have to continue searching after we have found it,
                    // because we want the _first_ occurrence.
                    val = probe;
                    high = probe;
                }
                else if (r < y)
                {
                    low = probe;
                }
                else
                {
                    high = probe;
                }
            }

            return val;
        }
    }
}
