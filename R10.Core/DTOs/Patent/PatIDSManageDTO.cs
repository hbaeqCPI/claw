using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using R10.Core.Entities.Patent;

namespace R10.Core.DTOs
{
    public class PatIDSManageDTO 
    {
        [Key]
        public int AppId { get; set; }

        public string CaseNumber { get; set; }
        [Display(Name = "Country")]
        public string Country { get; set; }

        [Display(Name = "Sub Case")]
        public string SubCase { get; set; }

        [Display(Name = "Case Type")]
        public string? CaseType { get; set; }

        [Display(Name = "Application Status")]
        public string? ApplicationStatus { get; set; }

        [Display(Name = "Application Status Date")]
        public DateTime? ApplicationStatusDate { get; set; }

        [Display(Name = "Cited")]
        public int CitedCount { get; set; }

        [Display(Name = "By")]
        public int ByCount { get; set; }

        [Display(Name = "Non-Pat")]
        public int NonPatCount { get; set; }

        [Display(Name = "Application No.")]
        public string? AppNumber { get; set; }

        [Display(Name = "Filing Date")]
        public DateTime? FilDate { get; set; }

        [Display(Name = "Patent No.")]
        public string? PatNumber { get; set; }

        [Display(Name = "Publication No.")]
        public string? PubNumber { get; set; }

        public string? FamilyNumber { get; set; }

        public int InvId { get; set; }
        public int? AgentID { get; set; }
        //public int? OwnerID { get; set; }
        public string? RespOffice { get; set; }

        [Display(Name = "Export Control?")]
        public bool? ExportControl { get; set; }

        [Display(Name = "Application Title")]
        public string? AppTitle { get; set; }

        public Invention Invention { get; set; }
        public PatApplicationStatus? PatApplicationStatus { get; set; }
        //public PatInventorApp? PatInventorApp { get; set; }

        public string? PubNumberSearch { get; set; }
        public string? PatNumberSearch { get; set; }


    }
}
