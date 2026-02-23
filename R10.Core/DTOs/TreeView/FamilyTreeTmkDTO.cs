using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace R10.Core.DTOs
{
    [Keyless]
    public class FamilyTreeTmkDTO
    {
        public int TmkId { get; set; }
        public string? CaseNumber { get; set; }

        [Display(Name = "Country")]
        public string? CountryName { get; set; }

        [Display(Name = "SubCase")]
        public string? SubCase { get; set; }

        [Display(Name = "Case Type")]
        public string? CaseType { get; set; }

        [Display(Name = "Trademark")]
        public string? TrademarkName { get; set; }

        [Display(Name = "Trademark Status")]
        public string? TrademarkStatus { get; set; }

        public string? ClientName { get; set; }

        [Display(Name = "Application No.")]
        public string? AppNumber { get; set; }

        [Display(Name = "Filing Date")]
        public DateTime? FilDate { get; set; }

        [Display(Name = "Registration No.")]
        public string? RegNumber { get; set; }

        [Display(Name = "Registration Date")]
        public DateTime? RegDate { get; set; }

        [NotMapped]
        public string? NodeType { get; set; }
    }
}
