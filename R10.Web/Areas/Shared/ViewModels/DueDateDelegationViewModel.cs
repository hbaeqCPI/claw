using R10.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class DueDateDelegationViewModel
    {
        public int? ActId { get; set; }
        public int? DDId { get; set; }
        [Display(Name = "Group")]
        public int? GroupId { get; set; }
        [Display(Name = "User")]
        public string? UserId { get; set; }
        [Display(Name = "Action Type")]
        public string? ActionType { get; set; }
        [Display(Name = "Base Date")]
        public DateTime? BaseDate { get; set; }
        [Display(Name = "Office Action")]
        public bool IsOfficeActionDue { get; set; }
        [Display(Name = "Responsible Attorney")]
        public string? ResponsibleAttorney { get; set; }
        [Display(Name = "Action Due")]
        public string? ActionDue { get; set; }
        [Display(Name = "Due Date")]
        public DateTime? DueDate { get; set; }
        [Display(Name = "Indicator")]
        public string? Indicator { get; set; }
        [Display(Name = "Due Date Attorney")]
        public string? DueDateAttorney { get; set; }
        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }

        public List<DueDateDelegationViewModelDetail>? CurrentDelegations { get; set; }
        public List<DueDateDelegationViewModelDetail>? DelegationOptions { get; set; }
    }

    public class DueDateDelegationViewModelDetail:BaseEntity
    {
        public int DelegationId { get; set; }
        public int? ActId { get; set; }
        public int? DDId { get; set; }
        [Display(Name = "Group")]
        public int? GroupId { get; set; }
        [Display(Name = "User")]
        public string? UserId { get; set; }
        public string? Id { get; set; }
        public string? Name { get; set; }
    }
}
