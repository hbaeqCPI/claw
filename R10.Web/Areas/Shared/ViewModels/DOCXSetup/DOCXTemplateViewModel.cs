
using DocumentFormat.OpenXml.Wordprocessing;
using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class DOCXTemplateViewModel
    {
        [Display(Name="Template File")]
        public string? TemplateFile { get; set; }

        [Display(Name="File Size")]
        public long FileSize { get; set; }

        public string? SystemType { get; set; }
    }
}
