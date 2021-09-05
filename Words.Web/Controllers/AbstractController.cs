#if NET40
#nullable enable
#endif

namespace Words.Web
{
#if NET
    using System.Data;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Caching.Memory;
    using Words.Web.Core;
#endif

    public abstract class AbstractController : Controller
    {
#if NET
        private readonly IMemoryCache memoryCache;
        private readonly IDbConnection connection;
        private readonly ILogger logger;
        private readonly WordFinders wordFinders;

        protected AbstractController(
            IMemoryCache memoryCache,
            IDbConnection connection,
            ILogger logger,
            WordFinders wordFinders)
        {
            this.memoryCache = memoryCache;
            this.connection = connection;
            this.logger = logger;
            this.wordFinders = wordFinders;
        }
#endif

        protected object? CacheGet(string cacheKey)
        {
#if NET
            return memoryCache.Get(cacheKey);
#else
            return HttpContext.Cache.Get($"query-{id}") as TResult?;
#endif
        }

        protected void CachePut(string key, object item, TimeSpan expirationDelay)
        {
#if NET
            MemoryCacheEntryOptions opts = new()
            {
                AbsoluteExpirationRelativeToNow = expirationDelay
            };
            opts.RegisterPostEvictionCallback(EvictionCallback);
            memoryCache.Set(key, item, opts);
#else
            _ = HttpContext.Cache.Add(
                key,
                item,
                null,
                Cache.NoAbsoluteExpiration,
                expirationDelay,
                CacheItemPriority.Normal,
                OnCacheItemRemoved);
#endif
        }

        protected async Task<TResult> Transact<TResult>(
            Func<IDbConnection, IDbTransaction, Task<TResult>> func,
            CancellationToken cancellationToken)
        {
#if NET
            IDbTransaction tran = connection.BeginTransaction();
            TResult result = await func.Invoke(connection, tran);
            tran.Commit();
            return result;
#else
            return await MvcApplication.Transact(
                func,
                cancellationToken);
#endif
        }

        protected async Task Transact(
            Func<IDbConnection, IDbTransaction, Task> action,
            CancellationToken cancellationToken)
        {
#if NET
            _ = await Transact(async (conn, tran) =>
            {
                await action.Invoke(conn, tran);
                return false;
            },
            cancellationToken);
#else
            await MvcApplication.Transact(
                action,
                cancellationToken);
#endif
        }

        protected List<Match> Matches(string text, SearchType searchType, int limit)
        {
#if NET
            return wordFinders.Matches(text, searchType, limit);
#else
            return MvcApplication.Matches(text, searchType, limit);
#endif
        }


#if NET
        private void EvictionCallback(object key, object value, EvictionReason reason, object state)
        {
            logger.CacheItemRemoved(key, reason, value);
        }
#else
        private void OnCacheItemRemoved(string key, object value, CacheItemRemovedReason reason)
        {
            LogManager.GetCurrentClassLogger().Info("Cache item {key} removed due to {reason}: {@value}", key, reason, value);
        }
#endif
    }
}
