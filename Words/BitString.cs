namespace Words
{
    using System.Collections.Generic;

    public class BitString
    {
        private static readonly byte[] MaskTop = {
            0x3f, 0x1f, 0x0f, 0x07, 0x03, 0x01, 0x00
        };

        private static readonly int[] BitsInByte =
        {
            0, 1, 1, 2, 1, 2, 2, 3, 1, 2, 2, 3, 2, 3, 3, 4, 1, 2, 2, 3, 2, 3, 3, 4, 2,
            3, 3, 4, 3, 4, 4, 5, 1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3, 4, 3, 4, 4, 5, 2, 3,
            3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6, 1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3,
            4, 3, 4, 4, 5, 2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6, 2, 3, 3, 4,
            3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6, 3, 4, 4, 5, 4, 5, 5, 6, 4, 5, 5, 6, 5,
            6, 6, 7, 1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3, 4, 3, 4, 4, 5, 2, 3, 3, 4, 3, 4,
            4, 5, 3, 4, 4, 5, 4, 5, 5, 6, 2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5,
            6, 3, 4, 4, 5, 4, 5, 5, 6, 4, 5, 5, 6, 5, 6, 6, 7, 2, 3, 3, 4, 3, 4, 4, 5,
            3, 4, 4, 5, 4, 5, 5, 6, 3, 4, 4, 5, 4, 5, 5, 6, 4, 5, 5, 6, 5, 6, 6, 7, 3,
            4, 4, 5, 4, 5, 5, 6, 4, 5, 5, 6, 5, 6, 6, 7, 4, 5, 5, 6, 5, 6, 6, 7, 5, 6,
            6, 7, 6, 7, 7, 8
        };

        private static readonly Dictionary<char, int> Base64Cache = new Dictionary<char, int> {
            { 'A' , 0 },
            { 'B' , 1 },
            { 'C' , 2 },
            { 'D' , 3 },
            { 'E' , 4 },
            { 'F' , 5 },
            { 'G' , 6 },
            { 'H' , 7 },
            { 'I' , 8 },
            { 'J' , 9 },
            { 'K' , 10 },
            { 'L' , 11 },
            { 'M' , 12 },
            { 'N' , 13 },
            { 'O' , 14 },
            { 'P' , 15 },
            { 'Q' , 16 },
            { 'R' , 17 },
            { 'S' , 18 },
            { 'T' , 19 },
            { 'U' , 20 },
            { 'V' , 21 },
            { 'W' , 22 },
            { 'X' , 23 },
            { 'Y' , 24 },
            { 'Z' , 25 },
            { 'a' , 26 },
            { 'b' , 27 },
            { 'c' , 28 },
            { 'd' , 29 },
            { 'e' , 30 },
            { 'f' , 31 },
            { 'g' , 32 },
            { 'h' , 33 },
            { 'i' , 34 },
            { 'j' , 35 },
            { 'k' , 36 },
            { 'l' , 37 },
            { 'm' , 38 },
            { 'n' , 39 },
            { 'o' , 40 },
            { 'p' , 41 },
            { 'q' , 42 },
            { 'r' , 43 },
            { 's' , 44 },
            { 't' , 45 },
            { 'u' , 46 },
            { 'v' , 47 },
            { 'w' , 48 },
            { 'x' , 49 },
            { 'y' , 50 },
            { 'z' , 51 },
            { '0' , 52 },
            { '1' , 53 },
            { '2' , 54 },
            { '3' , 55 },
            { '4' , 56 },
            { '5' , 57 },
            { '6' , 58 },
            { '7' , 59 },
            { '8' , 60 },
            { '9' , 61 },
            { '-' , 62 },
            { '_' , 63 }
        };
        private int length;
        private int W = 6;

        public BitString(string str)
        {
            Bytes = str;
            length = Bytes.Length * W;
        }

        public string Bytes { get; }

        public int Get(int p, int n)
        {
            // case 1: bits lie within the given byte
            if (p % W + n <= W)
            {
                var u = (ORD(Bytes[p / W]) & MaskTop[p % W]) >>
                        (W - p % W - n);
                return u;

                // case 2: bits lie incompletely in the given byte
            }
            else
            {
                var result = ORD(Bytes[p / W]) &
                             MaskTop[p % W];

                var l = W - p % W;
                p += l;
                n -= l;

                while (n >= W)
                {
                    result = (result << W) | ORD(Bytes[p / W]);
                    p += W;
                    n -= W;
                }

                if (n > 0)
                {
                    result = (result << n) | (ORD(Bytes[p / W]) >>
                                              (W - n));
                }

                return result;
            }
        }

        public int Count(int p, int n)
        {
            var count = 0;
            while (n >= 8)
            {
                count += BitsInByte[Get(p, 8)];
                p += 8;
                n -= 8;
            }

            return count + BitsInByte[Get(p, n)];
        }

        private int ORD(char c)
        {
            return Base64Cache[c];
        }
    }
}
