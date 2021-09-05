namespace Words.Web.Core
{
    public static class Extensions
    {
        private static readonly Action<ILogger, object, object, object, Exception?> cacheItemRemoved;

        static Extensions()
        {
            cacheItemRemoved = LoggerMessage.Define<object, object, object>(
                LogLevel.Information,
                new EventId(1, nameof(CacheItemRemoved)),
                "Cache item {Key} removed due to {Reason}: {@Value}");
        }

        public static void CacheItemRemoved(this ILogger logger, object key, object reason, object value)
        {
            cacheItemRemoved.Invoke(logger, key, reason, value, null);
        }
    }
}
