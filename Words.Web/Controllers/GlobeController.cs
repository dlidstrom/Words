#nullable enable

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

        [Route("globe1")]
        public ActionResult Index()
        {
            ViewModel model = new()
            {
                GlobeImage = "earth-blue-marble.jpg",
                BumpImage = "earth-topology.png"
            };
            return ViewGlobe(model);
        }

        [Route("globe2")]
        public ActionResult Globe2()
        {
            ViewModel model = new()
            {
                GlobeImage = "4_no_ice_clouds_mts_16k.jpg",
                BumpImage = "elev_bump_16k.jpg"
            };
            return ViewGlobe(model);
        }

        [Route("globe3")]
        public ActionResult Globe3()
        {
            ViewModel model = new()
            {
                GlobeImage = "4_no_ice_clouds_mts_8k.jpg",
                BumpImage = "elev_bump_16k.jpg"
            };
            return ViewGlobe(model);
        }

        [Route("globe4")]
        public ActionResult Globe4()
        {
            ViewModel model = new()
            {
                GlobeImage = "earth-large.jpg",
                BumpImage = "bump-large.jpg"
            };
            return ViewGlobe(model);
        }

        private ActionResult ViewGlobe(ViewModel model)
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
            ViewBag.count = MvcApplication.Transact((conn, tran) => conn.ExecuteScalar<int>("select count(*) from query", null, tran));
            return View("Index", model);
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

        public class ViewModel
        {
            public string GlobeImage { get; set; }

            public string BumpImage { get; set; }
        }
    }
}