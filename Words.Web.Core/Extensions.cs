namespace Words.Web.Core
{
    using static System.FormattableString;

    public static class Extensions
    {
        private static readonly Action<ILogger, object, object, object, Exception?> cacheItemRemoved;
        private static readonly Action<ILogger, string, double, Exception?> queryElapsed;

        static Extensions()
        {
            int eventId = 1;
            cacheItemRemoved = LoggerMessage.Define<object, object, object>(
                LogLevel.Information,
                new EventId(eventId++, nameof(CacheItemRemoved)),
                "Cache item {Key} removed due to {Reason}: {@Value}");

            queryElapsed = LoggerMessage.Define<string, double>(
                LogLevel.Information,
                new EventId(eventId++, nameof(QueryElapsed)),
                "Query '{Text}',{MilliSeconds:F2}");
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
