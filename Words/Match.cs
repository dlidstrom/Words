namespace Words
{
    public class Match
    {
        public string Value { get; set; }
        public MatchType Type { get; set; }

        public override string ToString()
        {
            return string.Format("{0} {1}", Value, Type);
        }
    }
}