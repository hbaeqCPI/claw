using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class DOCXListViewModel
    {
        public int DOCXId { get; set; }
        [Display(Name = "DOCX Name")]
        public string? DOCXName { get; set; }
        [Display(Name = "Template File")]
        public string? TemplateFile { get; set; }
        public string? SystemType { get; set; }
    }
}
