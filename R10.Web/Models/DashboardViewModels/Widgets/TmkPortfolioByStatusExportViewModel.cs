using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Models.DashboardViewModels
{
    public class TmkPortfolioImageExportViewModel
    {
        [Display(Name = "Case Number")]
        public string? CaseNumber { get; set; }

        [Display(Name = "Country")]
        public string? Country { get; set; }

        [Display(Name = "Sub Case")]
        public string? SubCase { get; set; }

        [Display(Name = "Case Type")]
        public string? CaseType { get; set; }

        [Display(Name = "Mark Type")]
        public string? MarkType { get; set; }

        [Display(Name = "Trademark Name")]
        public string? TrademarkName { get; set; }

        [Display(Name = "Status")]
        public string? Status { get; set; }

        [Display(Name = "Application No.")]
        public string? AppNumber { get; set; }

        [Display(Name = "Filing Date")]
        public DateTime? FilDate { get; set; }

        [Display(Name = "Publication No.")]
        public string? PubNumber { get; set; }

        [Display(Name = "Publication Date")]
        public DateTime? PubDate { get; set; }

        [Display(Name = "Registration No.")]
        public string? RegNumber { get; set; }

        [Display(Name = "Registration Date")]
        public DateTime? RegDate { get; set; }

        [Display(Name = "Last Renewal Date")]
        public DateTime? LastRenewalDate { get; set; }

        [Display(Name = "Next Renewal Date")]
        public DateTime? NextRenewalDate { get; set; }

        [Display(Name = "Image")]
        public string? ImageFile { get; set; }
    }

    public class TmkPortfolioByStatusExportViewModel
    {

        [Display(Name = "Case Number")]
        public string? CaseNumber { get; set; }

        [Display(Name = "Country")]
        public string? Country { get; set; }

        [Display(Name = "Sub Case")]
        public string? SubCase { get; set; }

        [Display(Name = "Case Type")]
        public string? CaseType { get; set; }

        [Display(Name = "Mark Type")]
        public string? MarkType { get; set; }

        [Display(Name = "Trademark Name")]
        public string? TrademarkName { get; set; }

        [Display(Name = "Status")]
        public string? Status { get; set; }       

        [Display(Name = "Application No.")]
        public string? AppNumber { get; set; }

        [Display(Name = "Filing Date")]
        public DateTime? FilDate { get; set; }

        [Display(Name = "Publication No.")]
        public string? PubNumber { get; set; }

        [Display(Name = "Publication Date")]
        public DateTime? PubDate { get; set; }

        [Display(Name = "Registration No.")]
        public string? RegNumber { get; set; }

        [Display(Name = "Registration Date")]
        public DateTime? RegDate { get; set; }        

        [Display(Name = "Last Renewal Date")]
        public DateTime? LastRenewalDate { get; set; }

        [Display(Name = "Next Renewal Date")]
        public DateTime? NextRenewalDate { get; set; }
    }
}
