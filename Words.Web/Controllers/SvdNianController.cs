namespace Words.Web.Controllers
{
    using System.Diagnostics;
    using System.Globalization;
    using System.Net;
    using System.Web.Mvc;
    using Models;
    using NLog;
    using ViewModels;

    public class SvdNianController : Controller
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        // GET: /SvdNian/
        public ActionResult Index()
        {
            return View(new NianQueryViewModel());
        }

        /// <summary>
        /// Perform a query.
        /// </summary>
        /// <param name="q"></param>
        /// <returns></returns>
        public ActionResult Search(NianQueryViewModel q)
        {
            if (q == null || !ModelState.IsValid || string.IsNullOrWhiteSpace(q.Text))
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json("Invalid request", JsonRequestBehavior.AllowGet);
            }

            var sw = Stopwatch.StartNew();
            var matches = MvcApplication.NianFinder.Nine(q.Text);
            sw.Stop();
            double millis = 1000.0 * sw.ElapsedTicks / Stopwatch.Frequency;
            int nodes = MvcApplication.WordFinder.Nodes;
            var results = new ResultsViewModel(q.Text, matches, millis, nodes);
            Log.Info(CultureInfo.InvariantCulture, "Nian query '{0}',{1},{2:F2}", q.Text, nodes, millis);

            // save query
            using (var session = MvcApplication.DocumentStore.OpenSession())
            {
                session.Store(new Query
                {
                    Type = QueryType.Nian,
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
