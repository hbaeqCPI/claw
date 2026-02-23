using System;
using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Patent
{
    public class CountryApplicationWebSvc : CountryApplicationWebSvcDetail
    {
        [Key]
        public int EntityId { get; set; }

        public int LogId { get; set; }
    }    

    public class CountryApplicationWebSvcDetail
    {
        [Required]
        [StringLength(25)]
        public string? CaseNumber { get; set; }

        [Required]
        [StringLength(5)]
        public string? Country { get; set; }

        [StringLength(8)]
        public string? SubCase { get; set; }

        [StringLength(3)]
        public string? CaseType { get; set; }

        [StringLength(15)]
        public string? ApplicationStatus { get; set; } = "Unfiled";

        [StringLength(255)]
        public string? AppTitle { get; set; }

        [StringLength(10)]
        public string? Agent { get; set; }

        [StringLength(60)]
        public string? AgentName { get; set; }

        [StringLength(20)]
        public string? PubNumber { get; set; }

        public DateTime? PubDate { get; set; }

        [StringLength(20)]
        public string? PCTNumber { get; set; }

        public DateTime? PCTDate { get; set; }

        [StringLength(10)]
        public string? RespOffice { get; set; }
    }
}
