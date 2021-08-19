#nullable enable

namespace Words
{
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

        private static readonly int[] Base64Cache = new int[123];

        static BitString()
        {
            Base64Cache['A'] = 0;
            Base64Cache['B'] = 1;
            Base64Cache['C'] = 2;
            Base64Cache['D'] = 3;
            Base64Cache['E'] = 4;
            Base64Cache['F'] = 5;
            Base64Cache['G'] = 6;
            Base64Cache['H'] = 7;
            Base64Cache['I'] = 8;
            Base64Cache['J'] = 9;
            Base64Cache['K'] = 10;
            Base64Cache['L'] = 11;
            Base64Cache['M'] = 12;
            Base64Cache['N'] = 13;
            Base64Cache['O'] = 14;
            Base64Cache['P'] = 15;
            Base64Cache['Q'] = 16;
            Base64Cache['R'] = 17;
            Base64Cache['S'] = 18;
            Base64Cache['T'] = 19;
            Base64Cache['U'] = 20;
            Base64Cache['V'] = 21;
            Base64Cache['W'] = 22;
            Base64Cache['X'] = 23;
            Base64Cache['Y'] = 24;
            Base64Cache['Z'] = 25;
            Base64Cache['a'] = 26;
            Base64Cache['b'] = 27;
            Base64Cache['c'] = 28;
            Base64Cache['d'] = 29;
            Base64Cache['e'] = 30;
            Base64Cache['f'] = 31;
            Base64Cache['g'] = 32;
            Base64Cache['h'] = 33;
            Base64Cache['i'] = 34;
            Base64Cache['j'] = 35;
            Base64Cache['k'] = 36;
            Base64Cache['l'] = 37;
            Base64Cache['m'] = 38;
            Base64Cache['n'] = 39;
            Base64Cache['o'] = 40;
            Base64Cache['p'] = 41;
            Base64Cache['q'] = 42;
            Base64Cache['r'] = 43;
            Base64Cache['s'] = 44;
            Base64Cache['t'] = 45;
            Base64Cache['u'] = 46;
            Base64Cache['v'] = 47;
            Base64Cache['w'] = 48;
            Base64Cache['x'] = 49;
            Base64Cache['y'] = 50;
            Base64Cache['z'] = 51;
            Base64Cache['0'] = 52;
            Base64Cache['1'] = 53;
            Base64Cache['2'] = 54;
            Base64Cache['3'] = 55;
            Base64Cache['4'] = 56;
            Base64Cache['5'] = 57;
            Base64Cache['6'] = 58;
            Base64Cache['7'] = 59;
            Base64Cache['8'] = 60;
            Base64Cache['9'] = 61;
            Base64Cache['-'] = 62;
            Base64Cache['_'] = 63;
        }

        private const int W = 6;

        public BitString(string str)
        {
            Bytes = str;
        }

        public string Bytes { get; }

        public int Get(int p, int n)
        {
            // case 1: bits lie within the given byte
            if (p % W + n <= W)
            {
                int u = (Base64Cache[Bytes[p / W]] & MaskTop[p % W]) >>
                        (W - p % W - n);
                return u;
            }

            // case 2: bits lie incompletely in the given byte
            int result = Base64Cache[Bytes[p / W]] &
                         MaskTop[p % W];

            int l = W - p % W;
            p += l;
            n -= l;

            while (n >= W)
            {
                result = (result << W) | Base64Cache[Bytes[p / W]];
                p += W;
                n -= W;
            }

            if (n > 0)
            {
                result = (result << n) | (Base64Cache[Bytes[p / W]] >>
                                          (W - n));
            }

            return result;
        }

        public int Count(int p, int n)
        {
            int count = 0;
            while (n >= 8)
            {
                count += BitsInByte[Get(p, 8)];
                p += 8;
                n -= 8;
            }

            return count + BitsInByte[Get(p, n)];
        }
    }
}
