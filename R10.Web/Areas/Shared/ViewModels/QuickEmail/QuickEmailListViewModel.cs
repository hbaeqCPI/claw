using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class QuickEmailListViewModel
    {
        public int QESetupId { get; set; }
        [Display(Name = "Template Name")]
        public string? TemplateName { get; set; }
        public string? SystemType { get; set; }
    }
}
