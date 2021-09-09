namespace Words.Web.Core
{
    using Microsoft.Extensions.Logging;
    using NLog;
    using NLog.Web;
    using Npgsql.Logging;

    public class Program
    {
        public static void Main(string[] args)
        {
            NpgsqlLogManager.IsParameterLoggingEnabled = true;
            NpgsqlLogManager.Provider = new SqlLoggingProvider();
            var logger = NLogBuilder.ConfigureNLog("NLog.config").GetCurrentClassLogger();
            try
            {
                logger.Debug("starting");
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "unhandled exception");
            }
            finally
            {
                logger.Debug("stopping");
                LogManager.Shutdown();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                })
                .UseNLog();
    }
}
