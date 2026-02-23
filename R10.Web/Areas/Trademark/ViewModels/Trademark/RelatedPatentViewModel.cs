using R10.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Trademark.ViewModels
{
    public class RelatedPatentViewModel:BaseEntity
    {
        public int RelatedTmkId { get; set; }
        public int AppId { get; set; }
        public int TmkId { get; set; }

        [Required]
        public string? RelatedCaseNumber { get; set; }

        [Display(Name = "Country")]
        public string? Country { get; set; }

        [Display(Name = "Sub Case")]
        public string? SubCase { get; set; }

        [Display(Name = "Case Type")]
        public string? CaseType { get; set; }

        [Display(Name = "Title")]
        public string? Title { get; set; }

    }
}
