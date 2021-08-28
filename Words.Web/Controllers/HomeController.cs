#nullable enable

namespace Words.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Caching;
    using System.Web.Mvc;
    using Dapper;
    using Models;
    using NLog;
    using ViewModels;
    using Words.Web.Entities;
    using Words.Web.Infrastructure;

    public class HomeController : Controller
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public async Task<ActionResult> Index(int? id, CancellationToken cancellationToken)
        {
            if (id != null)
            {
                string text = await MvcApplication.Transact(async (conn, tran) =>
                    await conn.QuerySingleAsync<string>("select text from query where query_id = @id", new { id }),
                    cancellationToken);
                if (HttpContext.Cache.Get($"query-{id}") is ResultsViewModel results)
                {
                    QueryViewModel cachedModel = new() { Text = text, Results = results };
                    if (results.Count == 0)
                    {
                        cachedModel.Recent = await GetRecentQueries(cancellationToken);
                    }

                    return View(cachedModel);
                }

                // perform search again
                Stopwatch sw = Stopwatch.StartNew();
                List<Match> matches = MvcApplication.Matches(text, SearchType.All, 100);
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
                    model.Recent = await GetRecentQueries(cancellationToken);
                }

                return View(model);
            }

            // get recent queries
            RecentQuery[] recentQueries = await GetRecentQueries(cancellationToken);
            return View(new QueryViewModel { Recent = recentQueries });
        }

        [HttpPost]
        public async Task<ActionResult> Search(QueryViewModel q, CancellationToken cancellationToken)
        {
            if (ModelState.IsValid == false || q.Text is null)
            {
                return View(q);
            }

            if (HttpContext.Cache.Get($"query-{Encoding.UTF8.GetBytes(q.Text).ComputeHash()}") is QueryId cachedQueryId)
            {
                return RedirectToAction(nameof(Index), new { id = cachedQueryId.Id });
            }

            Stopwatch sw = Stopwatch.StartNew();
            List<Match> matches = MvcApplication.Matches(q.Text, SearchType.All, 100);
            sw.Stop();
            ResultsViewModel results = new(q.Text, matches, sw.Elapsed.TotalMilliseconds);
            Log.Info(CultureInfo.InvariantCulture, "Query '{0}',{1:F2}", q.Text, sw.Elapsed.TotalMilliseconds);

            // save query
            int queryId = await MvcApplication.Transact(async (connection, tran) =>
            {
                int id = await connection.QuerySingleAsync<int>(@"
                    insert into query(type, text, elapsed_milliseconds, created_date, user_agent, user_host_address, browser_screen_pixels_height, browser_screen_pixels_width)
                    values (@type, @text, @elapsedmilliseconds, @createddate, @useragent, @userhostaddress::cidr, @browserscreenpixelsheight, @browserscreenpixelswidth)
                    returning query_id",
                    new Query(
                        Type: QueryType.Word.ToString(),
                        Text: q.Text,
                        ElapsedMilliseconds: (int)Math.Round(sw.Elapsed.TotalMilliseconds),
                        CreatedDate: DateTime.UtcNow,
                        UserAgent: Request.UserAgent,
                        UserHostAddress: Request.UserHostAddress,
                        BrowserScreenPixelsHeight: Request.Browser.ScreenPixelsHeight,
                        BrowserScreenPixelsWidth: Request.Browser.ScreenPixelsWidth),
                    tran);
                return id;
            },
            cancellationToken);

            _ = HttpContext.Cache.Add(
                $"query-{queryId}",
                results,
                null,
                Cache.NoAbsoluteExpiration,
                TimeSpan.FromDays(1),
                CacheItemPriority.Normal,
                OnCacheItemRemoved);

            _ = HttpContext.Cache.Add(
                $"query-{Encoding.UTF8.GetBytes(q.Text).ComputeHash()}",
                new QueryId(queryId, q.Text),
                null,
                Cache.NoAbsoluteExpiration,
                TimeSpan.FromDays(1),
                CacheItemPriority.Normal,
                OnCacheItemRemoved);

            return RedirectToAction(nameof(Index), new { id = queryId });
        }

        private async Task<RecentQuery[]> GetRecentQueries(CancellationToken cancellationToken)
        {
            if (HttpContext.Cache.Get("recent-queries") is RecentQuery[] cachedRecentQueries)
            {
                return cachedRecentQueries;
            }

            RecentQuery[] recentQueries =
                await MvcApplication.Transact(async (connection, tran) =>
                {
                    IEnumerable<RecentQuery> qs = await connection.QueryAsync<RecentQuery>(@"
                        SELECT q.query_id as queryid
                               , q.text
                               , q.created_date as createddate
                        FROM query q TABLESAMPLE BERNOULLI(1)");
                    return qs.ToArray().Randomize().Take(20).OrderBy(x => x.CreatedDate).ToArray();
                },
                cancellationToken);
            _ = HttpContext.Cache.Add(
                "recent-queries",
                recentQueries,
                null,
                Cache.NoAbsoluteExpiration,
                TimeSpan.FromDays(1),
                CacheItemPriority.Normal,
                OnCacheItemRemoved);
            return recentQueries;
        }

        private void OnCacheItemRemoved(string key, object value, CacheItemRemovedReason reason)
        {
            Log.Info("Cache item {key} removed due to {reason}: {@value}", key, reason, value);
        }

        private record QueryId(int Id, string Text);
    }
}
