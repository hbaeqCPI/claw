using R10.Core.Entities.Patent;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Core.Entities.GeneralMatter
{
    public class GMWorkflowAction : BaseEntity
    {
        [Key]
        public int ActId { get; set; }

        public int WrkId { get; set; }

        [Required]
        public int ActionTypeId { get; set; }

        [Required]
        public int ActionValueId { get; set; }

        public int OrderOfEntry { get; set; }

        [Display(Name = "Preview?")]
        public bool Preview { get; set; }

        [Display(Name = "Include Attachments?")]
        public bool IncludeAttachments { get; set; }

        [NotMapped]
        public byte[]? ParentTStamp { get; set; }

        public string? AttachmentFilter { get; set; }

        public GMWorkflow? Workflow { get; set; }
    }

    public enum GMWorkflowActionType
    {
        [Display(Name = "Send Email")]
        SendEmail,
        [Display(Name = "Create Action")]
        CreateAction,
        [Display(Name = "Close Action")]
        CloseAction,
        [Display(Name = "eSignature")]
        eSignature
    }


}
