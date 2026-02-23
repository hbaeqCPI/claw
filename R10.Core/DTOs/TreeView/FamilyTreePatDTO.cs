using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace R10.Core.DTOs
{
    [Keyless]
    public class FamilyTreePatDTO
    {
        public int InvId { get; set; }
        public int AppId { get; set; }
        public string? CaseNumber { get; set; }

        [Display(Name = "Family Number")]
        public string? FamilyNumber { get; set; }

        [Display(Name = "Country")]
        public string? CountryName { get; set; }

        [Display(Name = "SubCase")]
        public string? SubCase { get; set; }

        [Display(Name = "Case Type")]
        public string? CaseType { get; set; }

        [Display(Name = "Title")]
        public string? PatentTitle { get; set; }

        [Display(Name = "Status")]
        public string? PatentStatus { get; set; }

        public string? ClientName { get; set; }

        [Display(Name = "Application No.")]
        public string? AppNumber { get; set; }

        [Display(Name = "Filing Date")]
        public DateTime? FilDate { get; set; }

        [Display(Name = "Patent No.")]
        public string? PatNumber { get; set; }

        [Display(Name = "Issue Date")]
        public DateTime? IssDate { get; set; }

        [NotMapped]
        public string? NodeType { get; set; }

    }
}
