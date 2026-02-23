using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Patent
{
    public class PatTerminalDisclaimer : BaseEntity
    {
        [Key]
        public int TerminalDisclaimerId { get; set; }

        [Required]
        public int AppId { get; set; }

        [Display(Name = "Country")]
        public string? Country { get; set; }

        [Display(Name = "Patent Number")]
        public string? PatNumber { get; set; }

        [Display(Name = "Expiration Date")]
        public DateTime? ExpDate { get; set; }

        [Display(Name = "Status")]
        public string? ApplicationStatus { get; set; }

        public int? TerminalDisclaimerAppId { get; set; }
        public CountryApplication? CountryApplication { get; set; }
        public CountryApplication? TerminalDiscCountryApplication { get; set; }
    }
}
