namespace Words.Web.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.Threading;
    using Dapper;

    public static class JobTimer
    {
        private static readonly Timer _timer = new(OnTimerElapsed);
        private static readonly JobHost _jobHost = new();

        public static void Start()
        {
            int seconds = Convert.ToInt32(ConfigurationManager.AppSettings["JobIntervalSeconds"] ?? "3600");
            _ = _timer.Change(TimeSpan.Zero, TimeSpan.FromSeconds(seconds));
        }

        private static void OnTimerElapsed(object sender)
        {
            _jobHost.DoWork(RefreshQueryStatistics);
        }

        private static void RefreshQueryStatistics()
        {
            // get queries for analysis
            while (true)
            {
                QueryWithoutStatistics[] queriesWithoutStatistics = MvcApplication.Transact((connection, transaction) =>
                {
                    IEnumerable<QueryWithoutStatistics> query =
                        connection.Query<QueryWithoutStatistics>(@"
SELECT
    q.query_id
    , q.text
FROM
    query q
WHERE
    q.result_date IS NULL
ORDER BY
    q.created_date
LIMIT 25",
                        null,
                        transaction);
                    QueryWithoutStatistics[] result = query.ToArray();
                    return result;
                });

                if (queriesWithoutStatistics.Any() == false)
                {
                    break;
                }

                foreach (QueryWithoutStatistics query in queriesWithoutStatistics)
                {
                    List<Match> x = MvcApplication.Matches(query.Text, SearchType.Word, -1);
                }
            }
        }

        private class QueryWithoutStatistics
        {
            public int QueryId { get; set; }

            public string Text { get; set; } = null!;
        }
    }
}