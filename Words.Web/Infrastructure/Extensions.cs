#nullable enable

namespace Words.Web.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web.Hosting;
    using System.Web.Mvc;
    using Dapper;

    public static class UrlExtensions
    {
        public static string ContentCacheBreak(this UrlHelper url, string contentPath)
        {
            if (contentPath == null)
            {
                throw new ArgumentNullException(nameof(contentPath));
            }

            string path = HostingEnvironment.MapPath(contentPath);
            if (File.Exists(path) == false)
            {
                throw new Exception($"{path} does not exist");
            }

            string hashPart = string.Empty;
            if (path != null)
            {
                byte[] bytes = File.ReadAllBytes(path);
                string hash = bytes.ComputeHash();
                hashPart = $"?{hash}";
            }

            return $"{url.Content(contentPath)}{hashPart}";
        }

        public static string ComputeHash(this byte[] bytes)
        {
            using MD5 md5 = MD5.Create();
            byte[] hash = md5.ComputeHash(bytes);
            StringBuilder hashBuilder = new();
            foreach (byte b in hash)
            {
                _ = hashBuilder.Append($"{b:x2}");
            }

            return hashBuilder.ToString();
        }

        public static async Task<IEnumerable<T>> QueryAsync<T>(
            this IDbConnection connection,
            string sql,
            Func<T> _,
            object? param = null,
            IDbTransaction? transaction = null)
        {
            IEnumerable<T> result = await connection.QueryAsync<T>(sql, param, transaction);
            return result;
        }
    }
}