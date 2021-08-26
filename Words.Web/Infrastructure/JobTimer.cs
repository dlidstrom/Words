namespace Words.Web.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Dapper;
    using NLog;

    public static class JobTimer
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly Timer _timer = new(OnTimerElapsed);
        private static readonly JobHost _jobHost = new();

        public static void Start()
        {
            int seconds = Convert.ToInt32(ConfigurationManager.AppSettings["JobIntervalSeconds"] ?? "3600");
            _ = _timer.Change(TimeSpan.Zero, TimeSpan.FromSeconds(seconds));
        }

        private static async void OnTimerElapsed(object sender)
        {
            await _jobHost.DoWork(async t => await RefreshQueryStatistics(t));
        }

        private static async Task RefreshQueryStatistics(CancellationToken cancellationToken)
        {
            Logger.Info("refreshing query statistics");

            // get queries for analysis
            while (true)
            {
                var queriesWithoutStatistics =
                    await MvcApplication.Transact(async (connection, transaction) =>
                    {
                        var query =
                            await connection.QueryAsync(@"
SELECT
    q.query_id as QueryId
    , q.text
FROM
    query q
WHERE
    q.result_date IS NULL
    AND q.type = 'Word'
ORDER BY
    q.created_date
LIMIT 25",
                            () => new { QueryId = default(int), Text = default(string) },
                            null,
                            transaction);
                        var result = query.ToArray();
                        return result;
                    },
                    cancellationToken);

                if (queriesWithoutStatistics.Any() == false)
                {
                    Logger.Info("done");
                    break;
                }

                Logger.Info("found {queries} to update", queriesWithoutStatistics.Length);
                foreach (var query in queriesWithoutStatistics)
                {
                    List<Match> matches = MvcApplication.Matches(query.Text, SearchType.Word, 101);
                    await MvcApplication.Transact(
                        async (connection, transaction) =>
                        {
                            if (matches.Count <= 100)
                            {
                                var wordIdsQuery =
                                    await connection.QueryAsync(@"
SELECT w.text
       , w.word_id AS WordId
FROM word w
WHERE w.text = ANY(@matches)",
                                        () => new { Text = default(string), WordId = default(int) },
                                        new { Matches = matches.Select(x => x.Value).ToArray() },
                                        transaction);
                                Dictionary<string, int> wordDict = wordIdsQuery.ToDictionary(x => x.Text, x => x.WordId);
                                var pars = matches.Select(x => new { query.QueryId, WordId = wordDict[x.Value] }).ToArray();
                                _ = await connection.ExecuteAsync(@"
INSERT INTO result_word (query_id, word_id)
VALUES (@queryId, @wordId)
ON CONFLICT(query_id, word_id) DO NOTHING",
                                        pars,
                                        transaction);
                            }

                            _ = await connection.ExecuteAsync(@"
UPDATE query q
SET result_date = @date
WHERE q.query_id = @queryId",
                                new { query.QueryId, Date = DateTime.Now },
                                transaction);
                        },
                            cancellationToken);
                }
            }
        }
    }
}
