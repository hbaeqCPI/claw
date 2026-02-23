using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Core.Entities.Clearance
{
    public class TmcWorkflowAction : BaseEntity
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

        public TmcWorkflow? Workflow { get; set; }
    }

    public enum TmcWorkflowActionType
    {
        [Display(Name = "Send Email")]
        SendEmail
    }
}
