#if NET40
#nullable enable
#endif

namespace Words.Web
{
#if NET
    using System.Data;
    using Dapper;
    using Microsoft.Extensions.Caching.Memory;
#endif

    public abstract class AbstractController
    {
#if NET
        private readonly IMemoryCache memoryCache;
        private readonly IDbConnection connection;

        protected AbstractController(IMemoryCache memoryCache, IDbConnection connection) =>
            (this.memoryCache, this.connection) = (memoryCache, connection);
#endif

        protected object? CacheGet(string cacheKey)
        {
#if NET
            return memoryCache.Get(cacheKey);
#else
            return HttpContext.Cache.Get($"query-{id}") as TResult?;
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

#if !NET
        private void OnCacheItemRemoved(string key, object value, CacheItemRemovedReason reason)
        {
            Log.Info("Cache item {key} removed due to {reason}: {@value}", key, reason, value);
        }
#endif
    }
}
