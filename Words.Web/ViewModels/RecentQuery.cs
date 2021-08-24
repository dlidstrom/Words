#nullable enable

namespace Words.Web.ViewModels
{
    public class RecentQuery
    {
        public int QueryId { get; set; }

        public string Text { get; set; } = null!;
    }
}