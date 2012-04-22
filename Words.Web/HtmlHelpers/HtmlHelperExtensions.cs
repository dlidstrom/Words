namespace Words.Web.HtmlHelpers
{
    using System.Web.Mvc;

    public static class HtmlHelperExtensions
    {
        public static bool IsDebug(this HtmlHelper helper)
        {
            return MvcApplication.IsDebug;
        }
    }
}