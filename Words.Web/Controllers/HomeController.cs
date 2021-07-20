namespace Words.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Web.Caching;
    using System.Web.Mvc;
    using Dapper;
    using Models;
    using NLog;
    using ViewModels;

    public class HomeController : Controller
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public ActionResult Index(int? id)
        {
            if (id != null)
            {
                string text = MvcApplication.Transact((conn, tran) =>
                    conn.QuerySingle<string>("select text from query where query_id = @id", new { id }));
                if (HttpContext.Cache.Get($"query-{id}") is ResultsViewModel results)
                {
                    QueryViewModel cachedModel = new() { Text = text, Results = results };
                    if (results.Count == 0)
                    {
                        cachedModel.Recent = GetRecentQueries();
                    }

                    return View(cachedModel);
                }

                // perform search again
                Stopwatch sw = Stopwatch.StartNew();
                List<Match> matches = MvcApplication.WordFinder.Matches(text, 2);
                sw.Stop();
                results = new ResultsViewModel(text, matches, sw.Elapsed.TotalMilliseconds);
                _ = HttpContext.Cache.Add(
                    $"query-{id}",
                    results,
                    null,
                    Cache.NoAbsoluteExpiration,
                    TimeSpan.FromDays(1),
                    CacheItemPriority.Normal,
                    OnCacheItemRemoved);
                QueryViewModel model = new() { Text = text, Results = results };
                if (results.Count == 0)
                {
                    model.Recent = GetRecentQueries();
                }

                return View(model);
            }

            // get recent queries
            RecentQuery[] recentQueries = GetRecentQueries();
            return View(new QueryViewModel { Recent = recentQueries });
        }

        [HttpPost]
        public ActionResult Search(QueryViewModel q)
        {
            if (ModelState.IsValid == false)
            {
                return View(q);
            }

            Stopwatch sw = Stopwatch.StartNew();
            List<Match> matches = MvcApplication.WordFinder.Matches(q.Text, 2);
            sw.Stop();
            ResultsViewModel results = new(q.Text, matches, sw.Elapsed.TotalMilliseconds);
            Log.Info(CultureInfo.InvariantCulture, "Query '{0}',{1:F2}", q.Text, sw.Elapsed.TotalMilliseconds);

            // save query
            int queryId = MvcApplication.Transact((connection, tran) =>
            {
                int id = connection.QuerySingle<int>(@"
                    insert into query(type, text, elapsed_milliseconds, created_date)
                    values (@type, @text, @elapsedmilliseconds, @createddate)
                    returning query_id",
                    new
                    {
                        Type = QueryType.Word.ToString(),
                        q.Text,
                        ElapsedMilliseconds = (int)Math.Round(sw.Elapsed.TotalMilliseconds),
                        CreatedDate = DateTime.UtcNow
                    },
                    tran);
                return id;
            });

            _ = HttpContext.Cache.Add(
                $"query-{queryId}",
                results,
                null,
                Cache.NoAbsoluteExpiration,
                TimeSpan.FromDays(1),
                CacheItemPriority.Normal,
                OnCacheItemRemoved);

            return RedirectToAction(nameof(Index), new { id = queryId });
        }

        private RecentQuery[] GetRecentQueries()
        {
            RecentQuery[] recentQueries = MvcApplication.Transact((connection, tran) =>
            {
                return connection.Query<RecentQuery>(@"
                    select query_id as queryid
                           , text
                    from query
                    order by created_date desc
                    limit 20").ToArray();
            });
            return recentQueries;
        }

        private void OnCacheItemRemoved(string key, object value, CacheItemRemovedReason reason)
        {
            Log.Info("Cache item {key} removed due to {reason}", key, reason);
        }
    }
}
