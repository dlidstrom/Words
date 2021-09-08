namespace Words.Web
{
    using System.Collections.Generic;
    using Dapper;
    using Npgsql;

    public class WordFinders
    {
        private readonly IDictionary<int, WordFinder> wordFinders;

        public WordFinders(Bucket[] buckets)
        {
            wordFinders =
                buckets.ToDictionary(
                    x => x.Number,
                    x =>
                        WordFinder.CreateSuccinct(
                            x.Data,
                            Language.Swedish,
                            y => Array.Empty<string>(),
                            y => Array.Empty<string>()));
        }

        public List<Match> Matches(string text, SearchType searchType, int limit)
        {
            return new List<Match>();
        }

            private static string[] GetOriginal(NpgsqlConnection connection, string[] normalized)
            {
                IEnumerable<string> query =
                    connection.Query<string>(
                        "select original from normalized where normalized = any(@normalized)",
                        new { normalized });
                return query.ToArray();
            }

            private static string[] GetPermutations(NpgsqlConnection connection, string normalized)
            {
                IEnumerable<string> query =
                    connection.Query<string>(
                        "select permutation from permutation where normalized = @normalized",
                        new { normalized });
                return query.ToArray();
            }
    }
}
