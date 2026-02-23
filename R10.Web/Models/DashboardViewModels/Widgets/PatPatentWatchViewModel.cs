using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Models.DashboardViewModels
{
    public class PatPatentWatchViewModel
    {
        public string? Header { get; set; }

        [Display(Name = "Country")]
        public string? Country { get; set; }
        
        [Display(Name = "Title")]
        public string? Title { get; set; }
        
        [Display(Name = "Status")]
        public string? CurrentStatus { get; set; }

        [Display(Name = "Application No.")]
        public string? AppNo { get; set; }

        [Display(Name = "Publication No.")]
        public string? PubNo { get; set; }

        [Display(Name = "Patent No.")]
        public string? PatNo { get; set; }

        [Display(Name = "Filing Date")]
        public DateTime? FilDate { get; set; }

        [Display(Name = "Publication Date")]
        public DateTime? PubDate { get; set; }

        [Display(Name = "Issue Date")]
        public DateTime? IssDate { get; set; }

        [Display(Name = "Event Date")]
        public DateTime? EventDate { get; set; }

        [Display(Name = "Event Code")]
        public string? EventCode { get; set; }

        [Display(Name = "Event Description")]
        public string? EventDesc { get; set; }

        public string? LinkUrl { get; set; }
    }

    public class PatPatentWatchExportViewModel
    {
        [Display(Name = "Country")]
        public string? Country { get; set; }

        [Display(Name = "Title")]
        public string? Title { get; set; }

        [Display(Name = "Status")]
        public string? CurrentStatus { get; set; }

        [Display(Name = "Application No.")]
        public string? AppNo { get; set; }

        [Display(Name = "Publication No.")]
        public string? PubNo { get; set; }

        [Display(Name = "Patent No.")]
        public string? PatNo { get; set; }

        [Display(Name = "Filing Date")]
        public DateTime? FilDate { get; set; }

        [Display(Name = "Publication Date")]
        public DateTime? PubDate { get; set; }

        [Display(Name = "Issue Date")]
        public DateTime? IssDate { get; set; }

        [Display(Name = "Event Date")]
        public DateTime? EventDate { get; set; }

        [Display(Name = "Event Code")]
        public string? EventCode { get; set; }

        [Display(Name = "Event Description")]
        public string? EventDesc { get; set; }
    }
}
