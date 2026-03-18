using System;
using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class CountryExpCopyViewModel
    {
        public int CExpId { get; set; }

        [Display(Name = "Country")]
        [StringLength(5)]
        [Required]
        public string Country { get; set; }

        [Display(Name = "Case Type")]
        [StringLength(3)]
        [Required]
        public string CaseType { get; set; }

        [Display(Name = "Type")]
        [StringLength(20)]
        public string? Type { get; set; }

        [Display(Name = "Based On")]
        [StringLength(20)]
        public string? BasedOn { get; set; }

        [Display(Name = "Yr")]
        public int Yr { get; set; }

        [Display(Name = "Mo")]
        public int Mo { get; set; }

        [Display(Name = "Dy")]
        public int Dy { get; set; }

        [Display(Name = "Eff Based On")]
        [StringLength(20)]
        public string? EffBasedOn { get; set; }

        [Display(Name = "Eff Start Date")]
        public DateTime? EffStartDate { get; set; }

        [Display(Name = "Eff End Date")]
        public DateTime? EffEndDate { get; set; }
    }
}
