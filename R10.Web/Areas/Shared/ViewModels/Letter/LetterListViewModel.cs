using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class LetterListViewModel
    {
        public int LetId { get; set; }
        [Display(Name = "Letter Name")]
        public string? LetName { get; set; }
        [Display(Name = "Template File")]
        public string? TemplateFile { get; set; }
        public string? SystemType { get; set; }
    }
}
