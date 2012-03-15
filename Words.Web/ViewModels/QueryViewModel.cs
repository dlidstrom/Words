namespace Words.Web.ViewModels
{
    using System.ComponentModel.DataAnnotations;

    public class QueryViewModel
    {
        [Required(ErrorMessage = "*")]
        public string Text { get; set; }
    }
}
