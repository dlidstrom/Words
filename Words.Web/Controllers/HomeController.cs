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
        /// <returns></returns>
        public ActionResult Index()
        {
            return View(new QueryViewModel());
        }

        /// <summary>
        /// Perform a query.
        /// </summary>
        /// <param name="q"></param>
        /// <returns></returns>
        public PartialViewResult Search(QueryViewModel q)
        {
            if (q == null || !ModelState.IsValid || string.IsNullOrWhiteSpace(q.Text))
                return PartialView("_Results", null);

            var sw = Stopwatch.StartNew();
            var matches = MvcApplication.WordFinder.Matches(q.Text, 2);
            sw.Stop();
            double millis = 1000.0 * sw.ElapsedTicks / Stopwatch.Frequency;
            int nodes = MvcApplication.WordFinder.Nodes;
            var results = new ResultsViewModel(matches, millis);
            log.Info(CultureInfo.InvariantCulture, "Query '{0}',{1},{2:F2}", q.Text, nodes, millis);

            // save query
            using (var session = MvcApplication.DocumentStore.OpenSession())
            {
                session.Store(new Query
                {
                    Text = q.Text,
                    Nodes = nodes,
                    ElapsedMilliseconds = millis
                });

                session.SaveChanges();
            }

            return PartialView("_Results", results);
        }
    }
}
