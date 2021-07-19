using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web.Hosting;
using System.Web.Mvc;

namespace Words.Web.Infrastructure
{
    public static class UrlExtensions
    {
        public static string ContentCacheBreak(this UrlHelper url, string contentPath)
        {
            if (contentPath == null) throw new ArgumentNullException(nameof(contentPath));
            string path = HostingEnvironment.MapPath(contentPath);
            if (File.Exists(path) == false) throw new Exception($"{path} does not exist");
            string hashPart = string.Empty;
            if (path != null)
            {
                byte[] bytes = File.ReadAllBytes(path);
                using (MD5 md5 = MD5.Create())
                {
                    byte[] hash = md5.ComputeHash(bytes);
                    StringBuilder hashBuilder = new StringBuilder();
                    foreach (byte b in hash)
                    {
                        hashBuilder.Append($"{b:x2}");
                    }

                    hashPart = $"?{hashBuilder}";
                }
            }

            return $"{url.Content(contentPath)}{hashPart}";
        }
    }
}