using Microsoft.EntityFrameworkCore;
using R10.Core.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Core.DTOs
{
    [Keyless]
    public class DelegationEmailDTO
    {
        public int DelegationId { get; set; }
        public string? AssignedBy { get; set; }
        public string? AssignedTo { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }

    }

    [Keyless]
    public class DelegationDetailDTO
    {
        public int DelegationId { get; set; }
        public int? ActId { get; set; }
        public int? DDId { get; set; }
        public int? GroupId { get; set; }
        public string? UserId { get; set; }
        public int NotificationSent { get; set; }
        public int? ParentActId { get; set; }
        public int? ParentId { get; set; }
    }

    [Keyless]
    public class DelegationUserDTO
    {
        public string? UserId { get; set; }
        public string? DelegatedUser { get; set; }
    }
    public class DelegationGroupDTO
    {
        public int? GroupId { get; set; }
        public string? DelegatedGroup { get; set; }
    }

    [Keyless]
    public class DelegationActionTypeDTO
    {
        public string? ActionType { get; set; }
    }

    [Keyless]
    public class DelegationActionDueDTO
    {
        public string? ActionDue { get; set; }
    }

    [Keyless]
    public class DelegationIndicatorDTO
    {
        public string? Indicator { get; set; }
    }

    [Keyless]
    public class DelegationUtilityPreviewDTO
    {
        public int Id { get; set; }
        [Display(Name = "Due Date")]
        public DateTime? DueDate { get; set; }

        [Display(Name = "Action Type")]
        public string? ActionType { get; set; }

        [Display(Name = "Action Due")]
        public string? ActionDue { get; set; }

        [Display(Name = "Indicator")]
        public string? Indicator { get; set; }

        [Display(Name = "Delegated User")]
        public string? DelegatedUser { get; set; }

        [Display(Name = "Delegated Group")]
        public string? DelegatedGroup { get; set; }
        public string? UserId { get; set; }
        public int? GroupId { get; set; }

        public int? ActId { get; set; }
        
        public string CaseNumber { get; set; }

        [Display(Name = "Country")]
        public string? Country { get; set; }

        [Display(Name = "Sub Case")]
        public string? SubCase { get; set; }

        [NotMapped]
        public bool Selected { get; set; } = true;
    }

    [Keyless]
    public class DelegationUtilityCriteriaDTO
    {
        public string? Mode { get; set; }
        public DateTime? DueDateFrom { get; set; }
        public DateTime? DueDateTo { get; set; }
        public string? ActionType { get; set; }
        public string? ActionDue { get; set; }
        public string? Indicator { get; set; }
        public string? UserId { get; set; }
        public int? GroupId { get; set; }

        public DateTime? DueDateFromDelegate { get; set; }
        public DateTime? DueDateToDelegate { get; set; }
        public string? ActionTypeDelegate { get; set; }
        public string? ActionDueDelegate { get; set; }
        public string? IndicatorDelegate { get; set; }
        public string? UserIdDelegate { get; set; }
        public int? GroupIdDelegate { get; set; }
        public string? DelegateTo { get; set; }
    }

    [Keyless]
    public class DelegationUtilitySelectionDTO
    {
        public int Id { get; set; }
        public int? ActId { get; set; }
    }

    [Keyless]
    public class DelegationUtilityResultDTO
    {
        public string? NewDelegationIds { get; set; }
        public string? DeletedDelegationIds { get; set; }
    }

}
