using LawPortal.Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace LawPortal.Web.Areas.Shared.ViewModels
{
    public class ActionTypeViewModel : BaseEntity
    {
        public int ActionTypeID { get; set; }

        [Required]
        [StringLength(60)]
        [Display(Name = "Action Type")]
        public string? ActionType { get; set; }

        [Display(Name = "Country")]
        public string? Country { get; set; }

        public int? CDueId { get; set; }

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

        public string? CountryName { get; set; }
        public string? ResponsibleCode { get; set; }
        public string? ResponsibleName { get; set; }

        [Display(Name = "Office Action?")]
        public bool IsOfficeAction { get; set; }

        [Display(Name = "Active?")]
        public bool IsReminderOn { get; set; }

        [Display(Name = "Repeat Every")]
        public int ReminderRepeatInterval { get; set; }

        [Display(Name = "Recurrence")]
        public int ReminderRepeatRecurrence { get; set; }

        [Display(Name = "Repeat On")]
        public int ReminderRepeatOnDay { get; set; }

        [Display(Name = "Inventor")]
        public int ReminderInventorOpt { get; set; }

        [Display(Name = "Reviewer")]
        public int ReminderReviewerOpt { get; set; }
    }

    public class ActionTypeSearchResultViewModel 
    {
        public int ActionTypeID { get; set; }
        [Display(Name = "Action Type")]
        public string? ActionType { get; set; }
        [Display(Name = "Country")]
        public string? Country { get; set; }
        [Display(Name = "Office Action?")]
        public bool IsOfficeAction { get; set; }
    }
}
