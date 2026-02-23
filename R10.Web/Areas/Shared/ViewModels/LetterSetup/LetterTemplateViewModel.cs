
using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class LetterTemplateViewModel
    {
        [Display(Name= "Template File")]
        public string? TemplateFile { get; set; }

        [Display(Name = "File Size")]
        public long FileSize { get; set; }

        public string? SystemType { get; set; }
        public string? ContentType { get; set; }
        public string? Id { get; set; }
    }
}
