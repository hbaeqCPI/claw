using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Core.Entities.PatClearance
{
    public class PacWorkflowAction : BaseEntity
    {
        [Key]
        public int ActId { get; set; }

        public int WrkId { get; set; }

        [Required]
        public int ActionTypeId { get; set; }

        [Required]
        public int ActionValueId { get; set; }

        public int OrderOfEntry { get; set; }

        public bool Preview { get; set; }

        [Display(Name = "Include Attachments?")]
        public bool IncludeAttachments { get; set; }
        public string? AttachmentFilter { get; set; }

        [NotMapped]
        public byte[]? ParentTStamp { get; set; }

        public PacWorkflow? Workflow { get; set; }
    }

    public enum PacWorkflowActionType
    {
        [Display(Name = "Send Email")]
        SendEmail
    }
}
