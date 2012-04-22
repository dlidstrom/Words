namespace Words.Web.ViewModels
{
    using System.ComponentModel.DataAnnotations;

    public class NianQueryViewModel
    {
        [Required(ErrorMessage = "*"), StringLength(maximumLength: 9, MinimumLength = 9)]
        public string Text { get; set; }
    }
}
