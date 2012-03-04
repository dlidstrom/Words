using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Words.Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index(string q)
        {
            if (q == null)
                return View();

            if (string.IsNullOrWhiteSpace(q))
                return View(Enumerable.Empty<Words.Match>().ToList());

            return View(MvcApplication.WordFinder.Matches(q, 2));
        }
    }
}
