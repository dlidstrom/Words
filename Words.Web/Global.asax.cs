namespace Words.Web
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
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
#if DEBUG
        private const bool IsDebugFlag = true;
#else
        private const bool IsDebugFlag = false;
#endif
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private static readonly AutoResetEvent LoadingEvent = new AutoResetEvent(false);
        private static WordFinder wordFinder;
        private static NianFinder nianFinder;
        private static IDocumentStore documentStore;

        public static bool IsDebug { get { return IsDebugFlag; } }

        public static WordFinder WordFinder
        {
            get
            {
                if (wordFinder == null && !LoadingEvent.WaitOne(20000))
                    throw new Exception("Failed to load dictionary");
                return wordFinder;
            }
        }

        public static NianFinder NianFinder
        {
            get
            {
                if (nianFinder == null && !LoadingEvent.WaitOne(20000))
                    throw new Exception("Failed to load nian");
                return nianFinder;
            }
        }

        public static IDocumentStore DocumentStore
        {
            get
            {
                lock (typeof(MvcApplication))
                {
                    if (documentStore == null)
                    {
                        Log.Info("Initializing document store");
                        if (IsDebug)
                            documentStore = new DocumentStore { ConnectionStringName = "RavenDB" };
                        else
                        {
                            documentStore = new DocumentStore
                            {
                                ConnectionStringName = "RavenDB"
                            };
                        }

                        documentStore.Initialize();
                        Log.Info("Document store initialized");
                    }

                    return documentStore;
                }
            }
        }

        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new UserTrackerLogAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
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
            Task.Factory.StartNew(() => DocumentStore);
        }

        protected void Application_End()
        {
            Log.Info("Application ending");
            documentStore.Dispose();
        }

        private static void LoadDictionary()
        {
            Task.Factory.StartNew(() =>
                {
                    Log.Info("Loading dictionary");
                    object dir = AppDomain.CurrentDomain.GetData("DataDirectory");
                    string filename = Path.Combine(dir.ToString(), "words.txt");
                    string language = Path.Combine(dir.ToString(), "nian.txt");
                    wordFinder = new WordFinder(filename, Encoding.UTF8, Language.Swedish);
                    nianFinder = new NianFinder(language, Encoding.UTF8, new CultureInfo("sv-SE"));
                    LoadingEvent.Set();
                    Log.Info("Dictionary loaded");
                });
        }
    }
}