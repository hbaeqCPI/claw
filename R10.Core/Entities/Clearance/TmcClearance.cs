using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Core.Entities.Clearance
{
    public class TmcClearance : TmcClearanceDetail
    {
        public TmcClearanceStatus? TmcClearanceStatus { get; set; }
        public Client? Client { get; set; }
        public Attorney? Attorney { get; set; }        
        public List<TmcQuestion>? TmcQuestions { get; set; }
        public List<TmcClearanceStatusHistory>? TmcClearanceStatusesHistory { get; set; }
        public List<TmcKeyword>? Keywords { get; set; }
        public List<TmcList>? ListItems { get; set; }
        public List<TmcRelatedTrademark>? RelatedTrademarks { get; set; }
        public List<TmcDiscussion>? Discussions { get; set; }
        public List<TmcMark>? Marks { get; set; }

        [NotMapped]
        public string? CopyOptions { get; set; }
    }

    public class TmcClearanceDetail : BaseEntity
    {
        [Key]
        public int TmcId { get; set; }

        [Required]
        [StringLength(25)]
        public string CaseNumber { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Status")]
        public string ClearanceStatus { get; set; } = "Draft"; //Default, but is updated during save based on entered date.

        [Display(Name = "Status Date")]
        public DateTime? ClearanceStatusDate { get; set; }

        [Display(Name = "Date Requested")]
        public DateTime? DateRequested { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Requestor's Name")]
        public string? Requestor { get; set; }

        [Display(Name = "Division-Segment")]
        public int? ClientID { get; set; }

        [Display(Name = "Attorney")]
        public int? AttorneyID { get; set; }        

        [Display(Name = "North America")]
        public string? NAmerica { get; set; }

        [Display(Name = "Latin & Caribbean")]
        public string? CSAmerica { get; set; }

        [Display(Name = "Europe")]
        public string? Europe { get; set; }

        [Display(Name = "Middle East")]
        public string? MiddleEast { get; set; }

        [Display(Name = "Africa")]
        public string? Africa { get; set; }

        [Display(Name = "Asia")]
        public string? Asia { get; set; }

        [Display(Name = "Oceania")]
        public string? Oceana { get; set; }

        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }

        public string? UserId { get; set; }
    }
}
