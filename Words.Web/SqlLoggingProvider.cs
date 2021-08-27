#nullable enable

namespace Words.Web
{
    using System;
    using NLog;
    using Npgsql.Logging;

    internal class SqlLoggingProvider : INpgsqlLoggingProvider
    {
        public NpgsqlLogger CreateLogger(string name)
        {
            return new CustomLogger(LogManager.GetLogger(name));
        }

        private class CustomLogger : NpgsqlLogger
        {
            private readonly Logger logger;

            public CustomLogger(Logger logger)
            {
                this.logger = logger;
            }

            public override bool IsEnabled(NpgsqlLogLevel level)
            {
                return logger.IsEnabled(ToNLogLogLevel(level));
            }

            public override void Log(NpgsqlLogLevel level, int connectorId, string msg, Exception? exception = null)
            {
                LogEventInfo ev = new(ToNLogLogLevel(level), "", msg);
                if (exception != null)
                {
                    ev.Exception = exception;
                }

                if (connectorId != 0)
                {
                    ev.Properties["ConnectorId"] = connectorId;
                }

                logger.Log(ev);
            }

            private static LogLevel ToNLogLogLevel(NpgsqlLogLevel level)
            {
                return level switch
                {
                    NpgsqlLogLevel.Trace => LogLevel.Trace,
                    NpgsqlLogLevel.Debug => LogLevel.Debug,
                    NpgsqlLogLevel.Info => LogLevel.Info,
                    NpgsqlLogLevel.Warn => LogLevel.Warn,
                    NpgsqlLogLevel.Error => LogLevel.Error,
                    NpgsqlLogLevel.Fatal => LogLevel.Fatal,
                    _ => throw new ArgumentOutOfRangeException("level"),
                };
            }
        }
    }
}