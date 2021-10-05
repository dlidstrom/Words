namespace Words.Web.Core
{
    public static class Extensions
    {
        private static readonly Action<ILogger, string, Exception?> information;
        private static readonly Action<ILogger, string, Exception?> error;
        private static readonly Action<ILogger, object, object, object, Exception?> cacheItemRemoved;
        private static readonly Action<ILogger, string, double, Exception?> queryElapsed;

        static Extensions()
        {
            int eventId = 1;
            information = LoggerMessage.Define<string>(
                LogLevel.Information,
                new EventId(eventId++, nameof(Information)),
                "Informational message: {Information}");

            error = LoggerMessage.Define<string>(
                LogLevel.Error,
                new EventId(eventId++, nameof(Error)),
                "Error message: {Error}");

            cacheItemRemoved = LoggerMessage.Define<object, object, object>(
                LogLevel.Information,
                new EventId(eventId++, nameof(CacheItemRemoved)),
                "Cache item {Key} removed due to {Reason}: {@Value}");

            queryElapsed = LoggerMessage.Define<string, double>(
                LogLevel.Information,
                new EventId(eventId++, nameof(QueryElapsed)),
                "Query '{Text}',{MilliSeconds:F2}");
        }

        public static void Information(this ILogger logger, string message)
        {
            information.Invoke(logger, message, null);
        }

        public static void Error(this ILogger logger, string message)
        {
            error.Invoke(logger, message, null);
        }

        public static void CacheItemRemoved(this ILogger logger, object key, object reason, object value)
        {
            cacheItemRemoved.Invoke(logger, key, reason, value, null);
        }

        public static void QueryElapsed(this ILogger logger, string text, double milliseconds)
        {
            queryElapsed.Invoke(logger, text, milliseconds, null);
        }
    }
}
