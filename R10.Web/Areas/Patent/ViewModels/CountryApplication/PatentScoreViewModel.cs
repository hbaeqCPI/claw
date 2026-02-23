using R10.Core.DTOs;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Patent.ViewModels
{
    public class PatentScoreViewModel
    {
        public int AppId { get; set; }
        public double AverageRating { get; set; }
        public List<PatScoreDTO> PatScores { get; set; }

        public int ForwardCitationCount { get; set; }
        public int BackwardCitationCount { get; set; }
        public int ClaimCount { get; set; }
        public int OfficeActionCount { get; set; }
        public int SiblingCount { get; set; }


        public string CaseNumber { get; set; }

        [Display(Name = "Country")]
        public string Country { get; set; }

        [Display(Name = "Sub Case")]
        public string? SubCase { get; set; }
        
        [Display(Name = "Application Title")]
        public string? AppTitle { get; set; }

        [Display(Name = "Application No.")]
        public string? AppNumber { get; set; }

        [Display(Name = "Filing Date")]
        public DateTime? FilDate { get; set; }

        [Display(Name = "Patent No.")]
        public string? PatNumber { get; set; }

        [Display(Name = "Issue Date")]
        public DateTime? IssDate { get; set; }

    }

}
