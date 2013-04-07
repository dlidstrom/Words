namespace Words.Web.Models
{
    public enum QueryType
    {
        /// <summary>
        /// Word query type.
        /// </summary>
        Word,

        /// <summary>
        /// Svd Nian query type.
        /// </summary>
        Nian
    }

    public class Query
    {
        public Query()
        {
            Type = QueryType.Word;
        }

        public QueryType Type { get; set; }
        public string Text { get; set; }
        public int Nodes { get; set; }
        public double ElapsedMilliseconds { get; set; }
    }
}