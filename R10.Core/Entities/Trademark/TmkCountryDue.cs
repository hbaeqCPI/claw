using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Core.Entities.Trademark
{
    public class TmkCountryDue : ClawBaseEntity
    {
        [Key]
        public int CDueId { get; set; }

        [Required]
        [StringLength(5)]
        public string? Country { get; set; }

        [Required]
        [StringLength(3)]
        public string? CaseType { get; set; }

        [StringLength(30)]
        [Required(ErrorMessage = "The Action Type field is required.")]
        [Display(Name = "Action Type")]
        public string? ActionType { get; set; }

        [StringLength(30)]
        [Required(ErrorMessage = "Action Due is required.")]
        [Display(Name = "Action Due")]
        public string? ActionDue { get; set; }

        [StringLength(15)]
        [Required(ErrorMessage = "Based On is required.")]
        [Display(Name = "Based On")]
        public string? BasedOn { get; set; }

        [Display(Name = "Yr")]
        public int Yr { get; set; }

        [Display(Name = "Mo")]
        public int Mo { get; set; }

        [Display(Name = "Dy")]
        public int Dy { get; set; }

        [Required(ErrorMessage = "Indicator field is required.")]
        [StringLength(20)]
        [Display(Name = "Indicator")]
        public string? Indicator { get; set; }

        [Display(Name = "Recurring")]
        public float Recurring { get; set; }

        [Required(ErrorMessage = "The Eff Based On field is required.")]
        [StringLength(15)]
        [Display(Name = "Effective Period for")]
        public string? EffBasedOn { get; set; }

        [Display(Name = "From")]
        public DateTime? EffStartDate { get; set; }

        [Display(Name = "To")]
        public DateTime? EffEndDate { get; set; }

        public bool CPIAction { get; set; }

        [Display(Name = "Generate?")]
        public bool Calculate { get; set; }

        public int? CPIPermanentID { get; set; }

        [StringLength(500)]
        [Required(ErrorMessage = "At least one system must be selected.")]
        [Display(Name = "Systems")]
        public string Systems { get; set; } = "";

        [NotMapped]
        public bool IsNewRecord { get; set; }

        [NotMapped]
        public string? OriginalSystems { get; set; }

        [NotMapped]
        [Display(Name = "Follow up Action")]
        public string? FollowupAction { get; set; }

        [NotMapped]
        public string? OldFollowupAction { get; set; }

        [NotMapped]
        public string? RecurringDesc { get; set; }
    }

    public class BasedOnOption
    {
        public const string Allowance = "Allowance";
        public const string Filing = "Filing";
        public const string Priority = "Priority";
        public const string Publication = "Publication";
        public const string Registration = "Registration";
        public const string Renewal = "Renewal";
    }

    public enum RecurringOption
    {
        [Display(Name = "Non Recurring")]
        NonRecurring = 0,
        [Display(Name = "Based on Taken Date")]
        BasedOnTakenDate = 1,
        [Display(Name = "Based on Due Date")]
        BasedOnDueDate = -1
    }
}
