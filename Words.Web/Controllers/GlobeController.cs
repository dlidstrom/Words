namespace Words.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Web.Caching;
    using System.Web.Mvc;
    using Dapper;
    using NLog;

    public class GlobeController : Controller
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public ActionResult Index()
        {
            List<string> locations = new() { "lat,lng,pop" };
            if (HttpContext.Cache.Get("globe-data") is Location[] cachedData)
            {
                locations.AddRange(cachedData.Select(x => string.Format(CultureInfo.InvariantCulture, "{0},{1},1", x.Latitude, x.Longitude)));
            }
            else
            {
                Location[] data = MvcApplication.Transact((conn, tran) =>
                    conn.Query<Location>("select latitude, longitude from v_geolocation").ToArray());
                _ = HttpContext.Cache.Add(
                    "globe-data",
                    data,
                    null,
                    Cache.NoAbsoluteExpiration,
                    TimeSpan.FromDays(1),
                    CacheItemPriority.Normal,
                    OnCacheItemRemoved);
                locations.AddRange(data.Select(x => string.Format(CultureInfo.InvariantCulture, "{0},{1},1", x.Latitude, x.Longitude)));
            }

            ViewBag.data = string.Join("\\n", locations);
            return View();
        }

        private void OnCacheItemRemoved(string key, object value, CacheItemRemovedReason reason)
        {
            Log.Info("Cache item {key} removed due to {reason}: {@value}", key, reason, value);
        }

        private class Location
        {
            public double Latitude { get; set; }

            public double Longitude { get; set; }
        }
    }
}