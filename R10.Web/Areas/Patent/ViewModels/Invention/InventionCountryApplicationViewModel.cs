using System;
using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Patent.ViewModels
{
    public class InventionCountryApplicationViewModel
    {
        public int AppId { get; set; }
        public string? CaseNumber { get; set; }

        [Display(Name = "Country")]
        public string? Country { get; set; }

        [Display(Name = "Sub Case")]
        public string? SubCase { get; set; }

        [Display(Name = "Case Type")]
        public string? CaseType { get; set; }

        [Display(Name = "Status")]
        public string? ApplicationStatus { get; set; }

        [Display(Name = "Application No.")]
        public string? AppNumber { get; set; }

        [Display(Name = "Filing Date")]
        public DateTime? FilDate { get; set; }

        [Display(Name = "Patent Number")]
        public string? PatNumber { get; set; }

        [Display(Name = "Issue Date")]
        public DateTime? IssDate { get; set; }


        [Display(Name = "Created By")]
        public string? CreatedBy { get; set; }

        [Display(Name = "Updated By")]
        public string? UpdatedBy { get; set; }

        [Display(Name = "Date Created")]
        public DateTime? DateCreated { get; set; }

        [Display(Name = "Last Update")]
        public DateTime? LastUpdate { get; set; }
    }
}

