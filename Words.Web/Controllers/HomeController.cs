namespace Words.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Web.Mvc;
    using Dapper;
    using Models;
    using NLog;
    using Raven.Client.Documents.Session;
    using ViewModels;

    public class HomeController : Controller
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public ActionResult Index()
        {
            return View(new QueryViewModel());
        }

        [HttpPost]
        [ActionName("Index")]
        public ActionResult Search(QueryViewModel q)
        {
            if (ModelState.IsValid == false)
            {
                return View(q);
            }

            Stopwatch sw = Stopwatch.StartNew();
            List<Match> matches = MvcApplication.WordFinder.Matches(q.Text, 2);
            sw.Stop();
            ResultsViewModel results = new ResultsViewModel(q.Text, matches, sw.Elapsed.TotalMilliseconds);
            Log.Info(CultureInfo.InvariantCulture, "Query '{0}',{1:F2}", q.Text, sw.Elapsed.TotalMilliseconds);

            // save query
            MvcApplication.Transact((connection, tran) =>
            {
                int rows = connection.Execute(@"
                    insert into query(type, text, elapsed_milliseconds, created_date)
                    values (@type, @text, @elapsedmilliseconds, @createddate",
                    new
                    {
                        Type = QueryType.Word,
                        q.Text,
                        ElapsedMilliseconds = (int)Math.Round(sw.Elapsed.TotalMilliseconds),
                        CreatedDate = DateTime.UtcNow
                    },
                    tran);
            });
            using (IDocumentSession session = MvcApplication.DocumentStore.OpenSession())
            {
                session.Store(new Query
                {
                    Type = QueryType.Word,
                    Text = q.Text,
                    ElapsedMilliseconds = sw.Elapsed.TotalMilliseconds
                });

                session.SaveChanges();
            }

            return View(new QueryViewModel { Results = results });
        }
    }
}
