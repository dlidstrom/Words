namespace Words.Web.Controllers
{
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Web.Mvc;
    using NLog;
    using Words.Web.Models;
    using Words.Web.ViewModels;

    public class HomeController : Controller
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// See http://forums.asp.net/t/1671805.aspx/1.
        /// </summary>
        /// <param name="q"></param>
        /// <returns></returns>
        public ActionResult Index(string q)
        {
            if (q == null)
                return View();

            ResultsViewModel results;
            double millis = 0;
            int nodes = 0;
            if (string.IsNullOrWhiteSpace(q))
                results = new ResultsViewModel(Enumerable.Empty<Words.Match>(), 0);
            else
            {
                var sw = Stopwatch.StartNew();
                var matches = MvcApplication.WordFinder.Matches(q, 2);
                sw.Stop();
                millis = 1000.0 * sw.ElapsedTicks / Stopwatch.Frequency;
                nodes = MvcApplication.WordFinder.Nodes;
                results = new ResultsViewModel(matches, millis);
                log.Info(CultureInfo.InvariantCulture, "Query '{0}',{1},{2:F2}", q, nodes, millis);
            }

            // save query
            using (var session = MvcApplication.DocumentStore.OpenSession())
            {
                session.Store(new Query
                {
                    Text = q,
                    Nodes = nodes,
                    ElapsedMilliseconds = millis
                });

                session.SaveChanges();
            }

            return View(results);
        }
    }
}
