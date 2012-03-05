namespace Words.Web.Controllers
{
    using System.Diagnostics;
    using System.Linq;
    using System.Web.Mvc;
    using Words.Web.Models;
    using Words.Web.ViewModels;

    public class HomeController : Controller
    {
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
