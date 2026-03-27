using System;
using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class CountryDueCopyViewModel
    {
        public int CDueId { get; set; }

        [Display(Name = "Country")]
        [StringLength(5)]
        [Required]
        public string Country { get; set; }

        [Display(Name = "Case Type")]
        [StringLength(3)]
        [Required]
        public string CaseType { get; set; }

        [Display(Name = "Action Type")]
        [StringLength(60)]
        public string? ActionType { get; set; }

        [Display(Name = "Action Due")]
        [StringLength(60)]
        public string? ActionDue { get; set; }

        [Display(Name = "Based On")]
        [StringLength(20)]
        public string? BasedOn { get; set; }

        [Display(Name = "Yr")]
        public int Yr { get; set; }

        [Display(Name = "Mo")]
        public int Mo { get; set; }

        [Display(Name = "Dy")]
        public int Dy { get; set; }

        [Display(Name = "Indicator")]
        [StringLength(10)]
        public string? Indicator { get; set; }

        [Display(Name = "Recurring")]
        public float Recurring { get; set; }

        [Display(Name = "Eff Based On")]
        [StringLength(20)]
        public string? EffBasedOn { get; set; }

        [Display(Name = "Eff Start Date")]
        public DateTime? EffStartDate { get; set; }

        [Display(Name = "Eff End Date")]
        public DateTime? EffEndDate { get; set; }
    }
}
