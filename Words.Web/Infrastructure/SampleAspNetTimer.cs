[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(Words.Web.Infrastructure.JobTimer), "Start")]

namespace Words.Web.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;
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
            QueryWithoutStatistics[] queriesWithoutStatistics = MvcApplication.Transact(QueriesWithoutStatistics);
        }

        private static QueryWithoutStatistics[] QueriesWithoutStatistics(IDbConnection connection, IDbTransaction transaction)
        {
            IEnumerable<QueryWithoutStatistics> query =
                connection.Query<QueryWithoutStatistics>(@"
select
    q.query_id as queryid
    , q.text
from query q
left join query_statistic qs on qs.query_id = q.query_id
where qs.query_id is null",
                null,
                transaction);
            QueryWithoutStatistics[] result = query.ToArray();
            return result;
        }

        private class QueryWithoutStatistics
        {
            public int QueryId { get; set; }

            public string Text { get; set; } = null!;
        }
    }
}