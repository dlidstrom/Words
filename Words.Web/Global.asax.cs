namespace Words.Web
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Routing;
    using Infrastructure;
    using NLog;
    using Raven.Client;
    using Raven.Client.Document;

    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801
    public class MvcApplication : HttpApplication
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private static WordFinder wordFinder;
        private static NianFinder nianFinder;
        private static IDocumentStore documentStore;

        public static WordFinder WordFinder => wordFinder;

        public static NianFinder NianFinder => nianFinder;

        public static IDocumentStore DocumentStore => documentStore;

        private static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new UserTrackerLogAttribute());
        }

        private static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default",
                "{controller}/{action}/{id}",
                new { controller = "Home", action = "Index", id = UrlParameter.Optional });
        }

        protected void Application_Start()
        {
            Log.Info("Application starting");
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
            LoadDictionary();
            Log.Info("Initializing document store");
            documentStore = new DocumentStore
            {
                ConnectionStringName = "RavenDB"
            };

            documentStore.Initialize();
            Log.Info("Document store initialized");
        }

        protected void Application_End()
        {
            Log.Info("Application ending");
            documentStore.Dispose();
        }

        protected void Application_BeginRequest()
        {
            if (Context.IsDebuggingEnabled)
            {
                return;
            }

            if (Context.Request.IsSecureConnection == false
                && Context.Request.Url.ToString().Contains("localhost:") == false)
            {
                Response.Clear();
                Response.Status = "301 Moved Permanently";
                Response.AddHeader("Location", Context.Request.Url.ToString().Insert(4, "s"));
                Response.End();
            }
        }

        private static void LoadDictionary()
        {
            Log.Info("Loading dictionary");
            var dir = AppDomain.CurrentDomain.GetData("DataDirectory");
            var filename = Path.Combine(dir.ToString(), "words.txt");
            var language = Path.Combine(dir.ToString(), "nian.txt");
            wordFinder = new WordFinder(filename, Encoding.UTF8, Language.Swedish);
            nianFinder = new NianFinder(language, Encoding.UTF8, new CultureInfo("sv-SE"));
            Log.Info("Dictionary loaded");
        }
    }
}