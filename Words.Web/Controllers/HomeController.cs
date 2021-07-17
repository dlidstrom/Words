namespace Words.Web.Controllers
{
    using System.Diagnostics;
    using System.Globalization;
    using System.Web.Mvc;
    using Models;
    using NLog;
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

            var sw = Stopwatch.StartNew();
            var matches = MvcApplication.WordFinder.Matches(q.Text, 2);
            sw.Stop();
            var results = new ResultsViewModel(q.Text, matches, sw.Elapsed.TotalMilliseconds);
            Log.Info(CultureInfo.InvariantCulture, "Query '{0}',{1:F2}", q.Text, sw.Elapsed.TotalMilliseconds);

            // save query
            using (var session = MvcApplication.DocumentStore.OpenSession())
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
