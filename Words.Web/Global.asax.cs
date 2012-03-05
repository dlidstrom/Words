namespace Words.Web
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Routing;
    using NLog;
    using Raven.Client;
    using Raven.Client.Document;
    using Raven.Client.Embedded;
    using Words.Web.Infrastructure;

    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801
    public class MvcApplication : HttpApplication
    {
#if DEBUG
        private static readonly bool isDebug = true;
#else
        private static readonly bool isDebug = false;
#endif
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private static WordFinder wordFinder;
        private static AutoResetEvent loadingEvent = new AutoResetEvent(false);
        private static IDocumentStore documentStore;

        public static bool IsDebug { get { return isDebug; } }

        public static Words.WordFinder WordFinder
        {
            get
            {
                if (wordFinder == null && !loadingEvent.WaitOne(20000))
                    throw new Exception("Failed to load dictionary");
                return wordFinder;
            }
        }

        public static IDocumentStore DocumentStore
        {
            get
            {
                if (documentStore == null)
                {
                    log.Info("Initializing document store");
                    if (MvcApplication.IsDebug)
                        documentStore = new DocumentStore { ConnectionStringName = "RavenDB" };
                    else
                    {
                        documentStore = new EmbeddableDocumentStore
                        {
                            DataDirectory = Path.Combine(AppDomain.CurrentDomain.GetData("DataDirectory").ToString(), "Database")
                        };
                    }

                    documentStore.Initialize();
                    log.Info("Document store initialized");
                }

                return documentStore;
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
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional });
        }

        protected void Application_Start()
        {
            log.Info("Application starting");
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
            LoadDictionary();
        }

        protected void Application_End()
        {
            log.Info("Application ending");
            documentStore.Dispose();
        }

        private void LoadDictionary()
        {
            Task.Factory.StartNew(() =>
                {
                    log.Info("Loading dictionary");
                    object dir = AppDomain.CurrentDomain.GetData("DataDirectory");
                    string filename = Path.Combine(dir.ToString(), "words.txt");
                    wordFinder = new WordFinder(
                        filename,
                        Encoding.UTF8,
                        Language.Swedish);
                    loadingEvent.Set();
                    log.Info("Dictionary loaded");
                });
        }
    }
}