using R10.Core.Entities;
using R10.Core.Entities.Patent;
using R10.Core.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Patent.ViewModels
{
    public class TerminalDisclaimerViewModel: PatTerminalDisclaimer
    {
        [Display(Name = "Case Number")]
        public string? RelatedCaseNumber { get; set; }

        [Display(Name = "Sub Case")]
        public string? SubCase { get; set; }

        [Display(Name = "Case Type")]
        public string? CaseType { get; set; }

        [Display(Name = "Title")]
        public string? AppTitle { get; set; }

        [Display(Name = "Issue Date")]
        public DateTime? IssDate { get; set; }

        public bool ActiveSwitch { get; set; } = true;

        [Display(Name = "Current Expiration Date")]
        public DateTime? CurrentExpDate { get; set; }
    }

    public class TerminalDisclaimerSelectionViewModel 
    {
        public int? AppId { get; set; }

        public int? InvId { get; set; }

        public string CaseNumber { get; set; }
        [Display(Name = "Country")]
        public string? Country { get; set; }
        [Display(Name = "Sub Case")]
        public string? SubCase { get; set; }
        [Display(Name = "Case Type")]
        public string? CaseType { get; set; }

        [Display(Name = "Application No.")]
        public string? AppNumber { get; set; }

        [Display(Name = "Filing Date")]
        public DateTime? FilDate { get; set; }

        [Display(Name = "Publication No.")]
        public string? PubNumber { get; set; }

        [Display(Name = "Publication Date")]
        public DateTime? PubDate { get; set; }

        [StringLength(20)]
        [Display(Name = "Patent No.")]
        public string? PatNumber { get; set; }

        [Display(Name = "Issue Date")]
        public DateTime? IssDate { get; set; }

        [Display(Name = "Title")]
        public string? AppTitle { get; set; }

        [Display(Name = "Status")]
        public string? ApplicationStatus { get; set; }

        [Display(Name = "Expiration Date")]
        public DateTime? ExpDate { get; set; }


    }

}
