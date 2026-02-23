using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Core.Entities.PatClearance
{
    public class PacClearance : PacClearanceDetail
    {
        public PacClearanceStatus? PacClearanceStatus { get; set; }
        public Client? Client { get; set; }
        public Attorney? Attorney { get; set; }

        //public List<PacImage>? Images { get; set; }
        public List<PacQuestion>? PacQuestions { get; set; }
        public List<PacClearanceStatusHistory>? PacClearanceStatusesHistory { get; set; }
        
        public List<PacDiscussion>? Discussions { get; set; }

        public List<PacInventor>? Inventors { get; set; }

        public List<PacKeyword>? Keywords { get; set; }

        [NotMapped]
        public string? CopyOptions { get; set; }
    }

    public class PacClearanceDetail : BaseEntity
    {
        [Key]
        public int PacId { get; set; }

        [Required]
        [StringLength(25)]
        public string CaseNumber { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Status")]
        public string ClearanceStatus { get; set; } = "Draft"; //Default, but is updated during save based on entered date.

        [Display(Name = "Status Date")]
        public DateTime? ClearanceStatusDate { get; set; }

        [Required]
        [Display(Name = "Clearance Title")]
        [StringLength(255)]
        public string? ClearanceTitle { get; set; }

        [Display(Name = "Date Requested")]
        public DateTime? DateRequested { get; set; }
        
        [Display(Name = "Division-Segment")]        
        public int? ClientID { get; set; }

        [Display(Name = "Attorney")]
        public int? AttorneyID { get; set; }
        
        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }

        public string? UserId { get; set; }

        public int? DMSId { get; set; }
    }
}
