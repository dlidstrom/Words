namespace Words
{
    using System.Collections.Generic;

    public class BitString
    {
        private static readonly byte[] MaskTop = {
            0x3f, 0x1f, 0x0f, 0x07, 0x03, 0x01, 0x00
        };
        private static readonly Dictionary<char, uint> Base64Cache = new Dictionary<char, uint> {
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
        private readonly string bytes;
        private int length;
        private int W = 6;

        public BitString(string str)
        {
            bytes = str;
            length = bytes.Length * W;
        }

        public uint Get(int p, int n)
        {
            // case 1: bits lie within the given byte
            if (p % W + n <= W)
            {
                var u = (ORD(bytes[p / W | 0]) & MaskTop[p % W]) >>
                        (W - p % W - n);
                return u;

                // case 2: bits lie incompletely in the given byte
            }
            else
            {
                var result = ORD(bytes[p / W | 0]) &
                             MaskTop[p % W];

                var l = W - p % W;
                p += l;
                n -= l;

                while (n >= W)
                {
                    result = (result << W) | ORD(bytes[p / W | 0]);
                    p += W;
                    n -= W;
                }

                if (n > 0)
                {
                    result = (result << n) | (ORD(bytes[p / W | 0]) >>
                                              (W - n));
                }

                return result;
            }
        }

        private uint ORD(char c)
        {
            return Base64Cache[c];
        }
    }
}
