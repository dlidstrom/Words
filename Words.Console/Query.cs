#nullable enable

namespace Words.Console
{
    public class Query
    {
        public string? Type { get; set; }

        public string? Text { get; set; }

        public int Nodes { get; set; }

        public double ElapsedMilliseconds { get; set; }
    }
}
