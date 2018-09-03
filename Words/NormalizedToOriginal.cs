namespace Words
{
    public class NormalizedToOriginal
    {
        public NormalizedToOriginal()
        {
        }

        public NormalizedToOriginal(string normalized, string original)
        {
            Normalized = normalized;
            Original = original;
        }

        public string Normalized { get; private set; }

        public string Original { get; private set; }
    }
}
