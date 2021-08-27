#nullable enable

namespace Words.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Caching;
    using System.Web.Mvc;
    using Dapper;
    using NLog;

    public class GlobeController : Controller
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        [Route("globe1")]
        public async Task<ActionResult> Index(CancellationToken cancellationToken)
        {
            ViewModel model = new()
            {
                GlobeImage = "earth-blue-marble.jpg",
                BumpImage = "earth-topology.png"
            };
            return await ViewGlobe(model, cancellationToken);
        }

        [Route("globe2")]
        public async Task<ActionResult> Globe2(CancellationToken cancellationToken)
        {
            ViewModel model = new()
            {
                GlobeImage = "4_no_ice_clouds_mts_16k.jpg",
                BumpImage = "elev_bump_16k.jpg"
            };
            return await ViewGlobe(model, cancellationToken);
        }

        [Route("globe3")]
        public async Task<ActionResult> Globe3(CancellationToken cancellationToken)
        {
            ViewModel model = new()
            {
                GlobeImage = "4_no_ice_clouds_mts_8k.jpg",
                BumpImage = "elev_bump_16k.jpg"
            };
            return await ViewGlobe(model, cancellationToken);
        }

        [Route("globe4")]
        public async Task<ActionResult> Globe4(CancellationToken cancellationToken)
        {
            ViewModel model = new()
            {
                GlobeImage = "earth-large.jpg",
                BumpImage = "bump-large.jpg"
            };
            return await ViewGlobe(model, cancellationToken);
        }

        private async Task<ActionResult> ViewGlobe(ViewModel model, CancellationToken cancellationToken)
        {
            List<string> locations = new() { "lat,lng,pop" };
            if (HttpContext.Cache.Get("globe-data") is Location[] cachedData)
            {
                locations.AddRange(cachedData.Select(x => string.Format(CultureInfo.InvariantCulture, "{0},{1},1", x.Latitude, x.Longitude)));
            }
            else
            {
                Location[] data = await MvcApplication.Transact(async (conn, tran) =>
                    (await conn.QueryAsync<Location>("select latitude, longitude from v_geolocation")).ToArray(),
                    cancellationToken);
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
            ViewBag.count = await MvcApplication.Transact(async (conn, tran) =>
                await conn.ExecuteScalarAsync<int>("select count(*) from query", null, tran),
                cancellationToken);
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
            public string? GlobeImage { get; set; }

            public string? BumpImage { get; set; }
        }
    }
}