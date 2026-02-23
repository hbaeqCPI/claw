using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Core.Entities.DMS
{
    public class DMSWorkflow : BaseEntity
    {
        [Key]
        public int WrkId { get; set; }

        [Display(Name = "Workflow Name")]
        [Required, StringLength(100)]
        public string Workflow { get; set; }

        [Display(Name = "Description")]
        [StringLength(255)]
        public string? Description { get; set; }

        [Display(Name = "In Use?")]
        public bool ActiveSwitch { get; set; }

        [Required]
        public int TriggerTypeId { get; set; }

        [Required]
        public int TriggerValueId { get; set; }

        public bool? CPIWorkflow { get; set; }

        public string? ClientFilter { get; set; }

        public string? ReviewerEntityFilter { get; set; }


        public List<DMSWorkflowAction>? DMSWorkflowActions { get; set; }

        [NotMapped]
        public int[]? ClientFilterList { get; set; }

        [NotMapped]
        public int[]? ReviewerEntityFilterList { get; set; }
    }

    public enum DMSWorkflowTriggerType
    {
        [Display(Name = "Status Changed")]
        StatusChanged,

        [Display(Name = "New Discussion")]
        NewDiscussion,

        [Display(Name = "Discussion Reply")]
        DiscussionReply,

        [Display(Name = "Recommendation")]
        Recommendation,

        [Display(Name = "Inventor Changed")]
        InventorChanged,

        //[Display(Name = "Action Delegated")]
        //ActionDelegated,

        [Display(Name = "Ready For eSignature")]
        ReadyForESignature,

        [Display(Name = "Ready For Submission")]
        ReadyForSubmission,

        [Display(Name = "Inventor Award Generated")]
        InventorAwardGenerated,

        [Display(Name = "Inventor Award Paid")]
        InventorAwardPaid,
    }
}
