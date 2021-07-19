namespace Words.Web
{
    using System;
    using System.Configuration;
    using System.Data;
    using System.IO;
    using System.Text;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Routing;
    using Infrastructure;
    using LiteDB;
    using Newtonsoft.Json;
    using NLog;
    using Npgsql;
    using Raven.Client.Documents;
    using Logger = NLog.Logger;

    public class MvcApplication : HttpApplication
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private DatabaseWrapper databaseWrapper;

        public static WordFinder WordFinder { get; private set; }

        public static IDocumentStore DocumentStore { get; private set; }

        public static void Transact(Action<IDbConnection, IDbTransaction> action)
        {
            ConnectionStringSettings connectionString = ConfigurationManager.ConnectionStrings["Words"];
            using (IDbConnection connection = new NpgsqlConnection(connectionString.ConnectionString))
            {
                connection.Open();
                IDbTransaction tran = connection.BeginTransaction();
                action.Invoke(connection, tran);
                tran.Commit();
            }
        }

        private static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new UserTrackerLogAttribute());
        }

        private static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            _ = routes.MapRoute(
                "Default",
                "{controller}/{action}/{id}",
                new { controller = "Home", action = "Index", id = UrlParameter.Optional });
        }

        protected void Application_Start()
        {
            Log.Info("Application starting");

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);

            string appDataDirectory = AppDomain.CurrentDomain.GetData("DataDirectory").ToString();
            databaseWrapper = new DatabaseWrapper(Path.Combine(appDataDirectory, "words.db"));
            string filename = Path.Combine(appDataDirectory, "words.json");
            WordFinder = LoadWordFinder(filename);
            Log.Info("Dictionary loaded");
            DocumentStore = new DocumentStore
            {
                Urls = new[] { "http://localhost:8080" },
                Database = "Krysshjalpen"
            };

            _ = DocumentStore.Initialize();
            Log.Info("Document store initialized");
            GC.Collect();
        }

        protected void Application_End()
        {
            Log.Info("Application ending");
            DocumentStore?.Dispose();
            databaseWrapper?.Dispose();
        }

        protected void Application_BeginRequest()
        {
            if (Context.IsDebuggingEnabled || Context.Request.Url.ToString().Contains("localhost:"))
            {
                return;
            }

            if (Context.Request.IsSecureConnection == false)
            {
                Response.Clear();
                Response.Status = "301 Moved Permanently";
                Response.AddHeader("Location", Context.Request.Url.ToString().Insert(4, "s"));
                Response.End();
                return;
            }

            if (Context.Request.Url.Host == "krysshjalpen.se")
            {
                Response.Clear();
                Response.Status = "301 Moved Permanently";
                Response.AddHeader("Location", "https://krysshjälpen.se");
                Response.End();
            }
        }

        private WordFinder LoadWordFinder(string filename)
        {
            string succinctTreeDataJson = File.ReadAllText(filename, Encoding.UTF8);
            SuccinctTreeData succinctTreeData = JsonConvert.DeserializeObject<SuccinctTreeData>(succinctTreeDataJson);
            WordFinder wordFinder = WordFinder.CreateSuccinct(
                succinctTreeData,
                Language.Swedish,
                x => databaseWrapper.WordPermutations.FindById(new BsonValue(x))?.Words ?? new string[0],
                x => databaseWrapper.NormalizedToOriginals.FindById(new BsonValue(x))?.Original);
            return wordFinder;
        }
    }
}