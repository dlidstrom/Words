#if NET
#nullable enable
#endif

namespace Words.Web.Controllers
{
#if NET
    using System;
    using System.Data;
    using System.Diagnostics;
    using static System.FormattableString;
    using System.Text;
    using Dapper;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Caching.Memory;
    using Words.Web.Entities;
    using Words.Web.Infrastructure;
    using Words.Web.Models;
    using Words.Web.ViewModels;
    using Microsoft.Net.Http.Headers;
    using Words.Web.Core;
#else
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
#endif

    public class HomeController : AbstractController
    {
#if !NET
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
#else
        private readonly ILogger logger;

        public HomeController(
            IMemoryCache memoryCache,
            ILogger<HomeController> logger,
            IDbConnection connection,
            WordFinders wordFinders)
            : base(memoryCache, connection, logger, wordFinders)
        {
            this.logger = logger;
            logger.Information("here i am");
        }
#endif

        public async Task<ActionResult> Index(int? id, CancellationToken cancellationToken)
        {
            logger.Information("index daniel");
            if (id != null)
            {
                string text = await Transact(async (conn, tran) =>
                    await conn.QuerySingleAsync<string>("select text from query where query_id = @id", new { id }),
                    cancellationToken);
                if (CacheGet($"query-{id}") is ResultsViewModel results)
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
                List<Match> matches = Matches(text, SearchType.All, 100);
                sw.Stop();
                results = new ResultsViewModel(text, matches, sw.Elapsed.TotalMilliseconds);
                CachePut($"query-{id}", results, TimeSpan.FromDays(1));
                QueryViewModel model = new() { Text = text, Results = results };
                if (results.Count == 0)
                {
                    model.Recent = await GetRecentQueries(cancellationToken);
                }

                return View(model);
            }

            logger.Information("get recent queries");

            // get recent queries
            RecentQuery[] recentQueries = await GetRecentQueries(cancellationToken);
            return View(new QueryViewModel { Recent = recentQueries });
        }

#if NET
        [ValidateAntiForgeryToken]
        [HttpPost("search")]
#else
        [HttpPost]
#endif
        public async Task<ActionResult> Search(QueryViewModel q, CancellationToken cancellationToken)
        {
            logger.Information("begin search");
            if (ModelState.IsValid == false || q.Text is null)
            {
                logger.Information("view index");
                return View("Index", q);
            }

            if (CacheGet($"query-{Encoding.UTF8.GetBytes(q.Text).ComputeHash()}") is QueryId cachedQueryId)
            {
                logger.Information("cache found");
                return RedirectToAction(nameof(Index), new { id = cachedQueryId.Id });
            }

            Stopwatch sw = Stopwatch.StartNew();
            List<Match> matches = Matches(q.Text, SearchType.All, 100);
            sw.Stop();
            ResultsViewModel results = new(q.Text, matches, sw.Elapsed.TotalMilliseconds);
#if NET
            logger.QueryElapsed(q.Text, sw.Elapsed.TotalMilliseconds);
#else
            Log.Info(CultureInfo.InvariantCulture, "Query '{0}',{1:F2}", q.Text, sw.Elapsed.TotalMilliseconds);
#endif

            // save query
            int queryId = await Transact(async (connection, tran) =>
            {
                int id = await connection.QuerySingleAsync<int>(@"
                    insert into query(type, text, elapsed_milliseconds, created_date, user_agent, user_host_address, browser_screen_pixels_height, browser_screen_pixels_width)
                    values (@type, @text, @elapsedmilliseconds, @createddate, @useragent, @userhostaddress::cidr, @browserscreenpixelsheight, @browserscreenpixelswidth)
                    returning query_id",
#if NET
                    new Query(
                        Type: QueryType.Word.ToString(),
                        Text: q.Text,
                        ElapsedMilliseconds: (int)Math.Round(sw.Elapsed.TotalMilliseconds),
                        CreatedDate: DateTime.UtcNow,
                        UserAgent: Request.Headers[HeaderNames.UserAgent],
                        UserHostAddress: Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty,
                        BrowserScreenPixelsHeight: 480,
                        BrowserScreenPixelsWidth: 640),
#else
                    new Query(
                        Type: QueryType.Word.ToString(),
                        Text: q.Text,
                        ElapsedMilliseconds: (int)Math.Round(sw.Elapsed.TotalMilliseconds),
                        CreatedDate: DateTime.UtcNow,
                        UserAgent: Request.UserAgent,
                        UserHostAddress: Request.UserHostAddress,
                        BrowserScreenPixelsHeight: Request.Browser.ScreenPixelsHeight,
                        BrowserScreenPixelsWidth: Request.Browser.ScreenPixelsWidth),
#endif
                    tran);
                return id;
            },
            cancellationToken);

            CachePut(
                $"query-{queryId}",
                results,
                TimeSpan.FromDays(1));

            CachePut(
                $"query-{Encoding.UTF8.GetBytes(q.Text).ComputeHash()}",
                new QueryId(queryId, q.Text),
                TimeSpan.FromDays(1));

            return RedirectToAction(nameof(Index), new { id = queryId });
        }

        private async Task<RecentQuery[]> GetRecentQueries(CancellationToken cancellationToken)
        {
            if (CacheGet("recent-queries") is RecentQuery[] cachedRecentQueries)
            {
                return cachedRecentQueries;
            }

            RecentQuery[] recentQueries =
                await Transact(async (connection, tran) =>
                {
                    IEnumerable<RecentQuery> qs = await connection.QueryAsync<RecentQuery>(@"
                        SELECT q.query_id as queryid
                               , q.text
                               , q.created_date as createddate
                        FROM query q TABLESAMPLE BERNOULLI(1)");
                    return qs.ToArray().Randomize().Take(20).OrderBy(x => x.CreatedDate).ToArray();
                },
                cancellationToken);
            CachePut(
                "recent-queries",
                recentQueries,
                TimeSpan.FromDays(1));
            return recentQueries;
        }

        private record QueryId(int Id, string Text);
    }
}
