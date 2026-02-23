using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace R10.Core.DTOs
{
    [Keyless]
    public class TLSearchBiblioDTO
    {
        public int TLTmkId { get; set; }

        [Display(Name = "Application No.")]
        public string? AppNo { get; set; }
        [Display(Name = "Publication No.")]
        public string? PubNo { get; set; }
        [Display(Name = "Registration No.")]
        public string? RegNo { get; set; }
        [Display(Name = "Filing Date")]
        public DateTime? FilDate { get; set; }
        [Display(Name = "Publication Date")]
        public DateTime? PubDate { get; set; }
        [Display(Name = "Registration Date")]
        public DateTime? RegDate { get; set; }
        [Display(Name = "Allowance Date")]
        public DateTime? AllowanceDate { get; set; }
        [Display(Name = "Next Renewal Date")]
        public DateTime? NextRenewalDate { get; set; }

        [Display(Name = "Owner")]
        public string? Owner { get; set; }

        [Display(Name = "Owner Address")]
        public string? OwnerAddress { get; set; }

        [Display(Name = "Trademark Name")]
        public string? TrademarkName { get; set; }
        
        public string? ImageFileName { get; set; }


    }
}
