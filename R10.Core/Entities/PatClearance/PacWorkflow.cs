using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.PatClearance
{
    public class PacWorkflow : BaseEntity
    {
        [Key]
        public int WrkId { get; set; }

        [Display(Name = "Workflow Name")]
        [Required, StringLength(100)]
        public string Workflow { get; set; }

        [Display(Name = "Description")]
        [StringLength(255)]
        public string Description { get; set; }

        [Display(Name = "Active?")]
        public bool ActiveSwitch { get; set; }

        [Required]
        public int TriggerTypeId { get; set; }

        [Required]
        public int TriggerValueId { get; set; }

        public List<PacWorkflowAction>? WorkflowActions { get; set; }
    }

    public enum PacWorkflowTriggerType
    {
        [Display(Name = "Status Changed")]
        StatusChanged,
        [Display(Name = "New Discussion")]
        NewDiscussion,
        [Display(Name = "Discussion Reply")]
        DiscussionReply
    }
}

