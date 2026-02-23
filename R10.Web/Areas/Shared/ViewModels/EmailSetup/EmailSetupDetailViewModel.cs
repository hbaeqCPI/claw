using R10.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class EmailSetupDetailViewModel : EmailSetupDetail
    {
        public string? Name { get; set; }

        public string? Description { get; set; }

        [Display(Name = "Notification Type")]
        public string? ContentType { get; set; }

        [Display(Name = "Notification Type")]
        public string? ContentTypeDescription { get; set; }
        public string? LanguageCulture { get; set; }
    }
}
