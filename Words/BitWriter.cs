namespace Words
{
    using System.Collections.Generic;
    using System.Text;

    public class BitWriter
    {
        private const int W = 6;
        private readonly List<byte> bits = new List<byte>();
        private const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_";

        public void Write(int data, int numBits)
        {
            for (var i = numBits - 1; i >= 0; i--)
            {
                if ((data & (1u << i)) != 0)
                {
                    bits.Add(1);
                }
                else
                {
                    bits.Add(0);
                }
            }
        }

        public string GetData()
        {
            var chars = new StringBuilder();
            var b = 0;
            var i = 0;

            foreach (var t in bits)
            {
                b = (b << 1) | t;
                i += 1;
                if (i == W)
                {
                    var c = Chars[b];
                    chars.Append(c);
                    i = b = 0;
                }
            }

            if (i != 0)
            {
                chars.Append(Chars[b << (W - i)]);
            }

            return chars.ToString();
        }
    }
}
