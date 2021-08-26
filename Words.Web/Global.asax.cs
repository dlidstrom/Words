#nullable enable

namespace Words.Web
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Routing;
    using Dapper;
    using Infrastructure;
    using Newtonsoft.Json;
    using NLog;
    using Npgsql;
    using Logger = NLog.Logger;

    public class MvcApplication : HttpApplication
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private static IDictionary<int, WordFinder>? wordFinders;

        public static List<Match> Matches(string text, SearchType searchType, int limit)
        {
            int bucket = Bucket.ToBucket(text.Length);
            return wordFinders!.TryGetValue(bucket, out WordFinder? wordFinder)
                ? wordFinder.Matches(text, 0, searchType, limit)
                : throw new Exception($"no word finder found for word: {text}");
        }

        public static ITree Advanced(string text)
        {
            int bucket = Bucket.ToBucket(text.Length);
            return wordFinders!.TryGetValue(bucket, out WordFinder? wordFinder)
                ? wordFinder.Advanced
                : throw new Exception($"no word finder found for word: {text}");
        }

        [Obsolete]
        public static TResult Transact<TResult>(Func<IDbConnection, IDbTransaction, TResult> func)
        {
            ConnectionStringSettings connectionString = ConfigurationManager.ConnectionStrings["Words"];
            using IDbConnection connection = new NpgsqlConnection(connectionString.ConnectionString);
            connection.Open();
            IDbTransaction tran = connection.BeginTransaction();
            TResult result = func.Invoke(connection, tran);
            tran.Commit();
            return result;
        }

        [Obsolete]
        public static void Transact(Action<IDbConnection, IDbTransaction> action)
        {
            _ = Transact((conn, tran) =>
            {
                action.Invoke(conn, tran);
                return false;
            });
        }

        public static async Task<TResult> Transact<TResult>(
            Func<IDbConnection, IDbTransaction, Task<TResult>> func,
            CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                throw new Exception("cancellation requested");
            }

            ConnectionStringSettings connectionString = ConfigurationManager.ConnectionStrings["Words"];
            using IDbConnection connection = new NpgsqlConnection(connectionString.ConnectionString);
            connection.Open();
            IDbTransaction tran = connection.BeginTransaction();
            TResult result = await func.Invoke(connection, tran);
            tran.Commit();
            return result;
        }

        public static async Task Transact(
            Func<IDbConnection, IDbTransaction, Task> action,
            CancellationToken cancellationToken)
        {
            _ = await Transact(async (conn, tran) =>
            {
                await action.Invoke(conn, tran);
                return false;
            },
            cancellationToken);
        }

        private static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new UserTrackerLogAttribute());
        }

        private static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapMvcAttributeRoutes();

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
            string filename = Path.Combine(appDataDirectory, "words.json");
            string connectionString = ConfigurationManager.ConnectionStrings["Words"].ConnectionString;
            wordFinders = LoadWordFinders(filename, connectionString);
            Log.Info("Dictionary loaded");

            // read configuration here
            JobTimer.Start();
        }

        protected void Application_End()
        {
            Log.Info("Application ending");
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

        private IDictionary<int, WordFinder> LoadWordFinders(string filename, string connectionString)
        {
            string succinctTreeDataJson = File.ReadAllText(filename, Encoding.UTF8);
            Bucket[]? buckets = JsonConvert.DeserializeObject<Bucket[]>(succinctTreeDataJson);
            if (buckets is null)
            {
                throw new Exception("deserialization failed");
            }

            IDictionary<int, WordFinder> wordFinders =
                buckets.ToDictionary(
                    x => x.Number,
                    x =>
                        WordFinder.CreateSuccinct(
                            x.Data,
                            Language.Swedish,
                            y => GetPermutations(connectionString, y),
                            y => GetOriginal(connectionString, y)));
            return wordFinders;

            static string[] GetOriginal(string connectionString, string[] normalized)
            {
                using IDbConnection connection = new NpgsqlConnection(connectionString);
                connection.Open();
                IEnumerable<string> query =
                    connection.Query<string>(
                        "select original from normalized where normalized = any(@normalized)",
                        new { normalized });
                return query.ToArray();
            }

            static string[] GetPermutations(string connectionString, string normalized)
            {
                using IDbConnection connection = new NpgsqlConnection(connectionString);
                IEnumerable<string> query =
                    connection.Query<string>(
                        "select permutation from permutation where normalized = @normalized",
                        new { normalized });
                return query.ToArray();
            }
        }
    }
}