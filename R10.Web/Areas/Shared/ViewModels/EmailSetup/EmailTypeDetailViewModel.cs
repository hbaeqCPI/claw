using R10.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class EmailTypeDetailViewModel : EmailTypeDetail
    {
        public string? TemplateName { get; set; }

        [Display(Name = "Notification Type")]
        public string? ContentTypeDescription { get; set; }

        public string? Policy { get; set; }
    }
}
