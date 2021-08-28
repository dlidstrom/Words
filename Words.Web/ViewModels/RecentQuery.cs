#nullable enable

namespace Words.Web.ViewModels
{
    using System;

    public class RecentQuery
    {
        public int QueryId { get; set; }

        public string Text { get; set; } = null!;

        public DateTime CreatedDate { get; set; }
    }
}