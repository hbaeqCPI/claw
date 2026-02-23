using System;
using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class DocCtryAppViewModel
    {
        public int AppId { get; set; }

        public string? CaseNumber { get; set; }

        [Display(Name = "Country")]
        public string? Country { get; set; }

        [Display(Name = "Sub Case")]
        public string? SubCase { get; set; }

        [Display(Name = "Case Type")]
        public string? CaseType { get; set; }

        public string? ClientName { get; set; }

        [Display(Name = "Application Title")]
        public string? AppTitle { get; set; }

        [Display(Name = "Attorney 1")]
        public string? Attorney1 { get; set; }
        [Display(Name = "Attorney 2")]
        public string? Attorney2 { get; set; }
        [Display(Name = "Attorney 3")]
        public string? Attorney3 { get; set; }

        [Display(Name = "Status")]
        public string? ApplicationStatus { get; set; }

        [Display(Name = "Status Date")]
        public DateTime? ApplicationStatusDate { get; set; }

        [Display(Name = "Parent Application No.")]
        public string? ParentAppNumber { get; set; }

        [Display(Name = "Parent Filing Date")]
        public DateTime? ParentFilDate { get; set; }

        [Display(Name = "Parent Filing Country")]
        public string? ParentFilCountry { get; set; }

        [Display(Name = "Parent Patent No.")]
        public string? ParentPatNumber { get; set; }

        [Display(Name = "Parent Issue Date")]
        public DateTime? ParentIssDate { get; set; }

        [Display(Name = "PCT No.")]
        public string? PCTNumber { get; set; }

        [Display(Name = "PCT Date")]
        public DateTime? PCTDate { get; set; }

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

        [Display(Name = "Expiration Date")]
        public DateTime? ExpDate { get; set; }

        //[Display(Name = "Remarks")]
        //public string? Remarks { get; set; }
    }
}
