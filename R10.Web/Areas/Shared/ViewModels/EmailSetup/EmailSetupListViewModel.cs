
using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class EmailSetupListViewModel
    {
        public int EmailSetupId { get; set; }
        public int EmailTypeId { get; set; }

        [Display(Name = "Language")]
        public string? Language { get; set; }
        
        public bool Default { get; set; }
        
        [Display(Name = "Subject")]
        public string? Subject { get; set; }

        public string? LanguageCulture { get; set; }
    }
}
