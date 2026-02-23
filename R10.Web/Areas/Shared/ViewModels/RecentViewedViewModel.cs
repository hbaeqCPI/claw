using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class RecentViewedViewModel
    {
        [Display(Name = "Activity Date")]
        public DateTime? ActivityDate { get; set; }

        [Display(Name = "User Id")]
        public string? UserId { get; set; }

        public int Id { get; set; }
        public string? IdType { get; set; }

        [Display(Name = "System")]
        public string? System { get; set; }

        [Display(Name = "Title")]
        public string? Title { get; set; }
        public string? CaseNumber { get; set; }

        [Display(Name = "Country")]
        public string? Country { get; set; }
        [Display(Name = "Sub Case")]
        public string? SubCase { get; set; }  
        [Display(Name = "Status")]
        public string? Status { get; set; }

        [Display(Name = "Application No.")]
        public string? AppNumber { get; set; }
        [Display(Name = "Filing Date")]
        public DateTime? FilDate { get; set; }
        [Display(Name = "Patent/Reg No.")]
        public string? PatRegNumber { get; set; }
        [Display(Name = "Issue/Reg Date")]
        public DateTime? IssRegDate { get; set; }

    }
}
