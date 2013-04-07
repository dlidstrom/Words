namespace Words.Web.Controllers
{
    using System.Diagnostics;
    using System.Globalization;
    using System.Net;
    using System.Web.Mvc;
    using Models;
    using NLog;
    using ViewModels;

    public class HomeController : Controller
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

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
        public ActionResult Search(QueryViewModel q)
        {
            if (q == null || !ModelState.IsValid || string.IsNullOrWhiteSpace(q.Text))
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json("Invalid request", JsonRequestBehavior.AllowGet);
            }

            var sw = Stopwatch.StartNew();
            var matches = MvcApplication.WordFinder.Matches(q.Text, 2);
            sw.Stop();
            double millis = 1000.0 * sw.ElapsedTicks / Stopwatch.Frequency;
            int nodes = MvcApplication.WordFinder.Nodes;
            var results = new ResultsViewModel(q.Text, matches, millis, nodes);
            Log.Info(CultureInfo.InvariantCulture, "Query '{0}',{1},{2:F2}", q.Text, nodes, millis);

            // save query
            using (var session = MvcApplication.DocumentStore.OpenSession())
            {
                session.Store(new Query
                {
                    Type = QueryType.Word,
                    Text = q.Text,
                    Nodes = nodes,
                    ElapsedMilliseconds = millis
                });

                session.SaveChanges();
            }

            return Json(results, JsonRequestBehavior.AllowGet);
        }
    }
}
