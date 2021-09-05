#nullable enable

namespace Words.Web.ViewModels
{
    using System.ComponentModel.DataAnnotations;

    public class QueryViewModel
    {
        public QueryViewModel()
        {
#if NET
            Recent = Array.Empty<RecentQuery>();
#else
            Recent = new RecentQuery[0];
#endif
        }

        [Required(ErrorMessage = "*")]
        [MinLength(1)]
        [MaxLength(255)]
        public string? Text { get; set; }

        public ResultsViewModel? Results { get; set; }

        public RecentQuery[] Recent { get; set; }
    }
}
