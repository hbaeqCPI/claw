using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Patent.ViewModels
{
    public class PatentWatchExportViewModel
    {
        [Display(Name = "Country")]
        public string? Country { get; set; }

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

        [Display(Name = "Title")]
        public string? Title { get; set; }

        [Display(Name = "Owner")]
        public string? OwnerName { get; set; }

        [Display(Name = "Current Status")]
        public string? CurrentStatus { get; set; }

        [Display(Name = "Latest Event Date")]
        public DateTime? EventDate { get; set; }

        [Display(Name = "Latest Legal Status Event")]
        public string? EventCode { get; set; }

        [Display(Name = "Event Desc")]
        public string? EventDesc { get; set; }

        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }

        [Display(Name = "Keywords")]
        public string? Keywords { get; set; }
    }

    public class PatentWatchListExportViewModel
    {
        [Display(Name = "Number Type")]
        public string? NumberType { get; set; }

        [Display(Name = "Number")]
        public string? Number { get; set; }

    
        [Display(Name = "Current Status")]
        public string? CurrentStatus { get; set; }

        [Display(Name = "Latest Legal Status Event")]
        public string? EventCode { get; set; }

        [Display(Name = "Event Desc")]
        public string? EventDesc { get; set; }

        [Display(Name = "Latest Event Date")]
        public DateTime? EventDate { get; set; }

        [Display(Name = "Filing Date")]
        public DateTime? FilDate { get; set; }

        [Display(Name = "Created By")]
        public string? CreatedBy { get; set; }

        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }

        [Display(Name = "Keywords")]
        public string? Keywords { get; set; }
    }

    
    

    public class PatentWatchUpdateViewModel
    {
        public int WatchId { get; set; }
        public string? Remarks { get; set; }
        public string? Keywords { get; set; }
    }

}
