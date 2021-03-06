﻿namespace Words.Web.ViewModels
{
    using System.ComponentModel.DataAnnotations;

    public class QueryViewModel
    {
        [Required(ErrorMessage = "*")]
        [MinLength(1)]
        public string Text { get; set; }

        public ResultsViewModel Results { get; set; }
    }
}
