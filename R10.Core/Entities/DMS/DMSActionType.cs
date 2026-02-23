using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace R10.Core.Entities.DMS
{
    public class DMSActionType : DMSActionTypeDetail
    {
        public List<DMSActionParameter>? ActionParameters { get; set; }
        public Attorney? Responsible { get; set; }
        
        [NotMapped]
        public string? CopyOptions { get; set; }
    }

    public class DMSActionTypeDetail : BaseEntity
    {
        [Key]
        public int ActionTypeID { get; set; }

        [Required]
        [StringLength(60)]
        [Display(Name = "Action Type")]
        public string? ActionType { get; set; }

        [StringLength(60)]
        [Display(Name = "Follow Up Action")]
        public string? FollowUpMsg { get; set; }

        [Required]
        [Display(Name = "Month")]
        public int FollowUpMonth { get; set; }

        [Required]
        [Display(Name = "Day")]
        public int FollowUpDay { get; set; }

        [Display(Name = "Indicator")]
        public string? FollowUpIndicator { get; set; }

        [Display(Name = "Follow up Based On")]
        public short FollowUpGen { get; set; }

        [Display(Name = "Responsible Attorney")]
        public int? ResponsibleID { get; set; }

        public string? Remarks { get; set; }

        [Display(Name = "Active?")]
        public bool IsReminderOn { get; set; }

        [Display(Name = "Repeat Every")]
        public int ReminderRepeatInterval { get; set; }

        [Display(Name = "Recurrence")]
        public int ReminderRepeatRecurrence { get; set; }       //1-days, 2-weeks, 3-months

        [Display(Name = "Repeat On")]
        public int ReminderRepeatOnDay { get; set; }            //1-Monday,2-Tue,3-Wed,4-Thu,5-Fri,6-Sat,7-Sun

        [Display(Name = "Inventor")]
        public int ReminderInventorOpt { get; set; }            //0-None,1-All,2-DefaultInventor

        [Display(Name = "Reviewer")]
        public int ReminderReviewerOpt { get; set; }            //0-None,1-All
    }
}
