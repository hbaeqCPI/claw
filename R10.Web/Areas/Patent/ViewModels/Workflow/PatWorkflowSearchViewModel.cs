using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Web.Areas.Patent.ViewModels
{
    public class PatWorkflowSearchViewModel
    {
        public int WrkId { get; set; }

        [Display(Name = "Workflow")]
        public string? Workflow { get; set; }

        [Display(Name = "Description")]
        public string? Description { get; set; }

        public int TriggerTypeId { get; set; }

        [Display(Name = "Trigger")]
        public string? Trigger { get; set; }

        [Display(Name = "Trigger Value")]
        public string? TriggerValue { get; set; }

        [Display(Name = "Created By")]
        public string? CreatedBy { get; set; }

        [Display(Name = "Updated By")]
        public string? UpdatedBy { get; set; }

        [Display(Name = "Created On")]
        public DateTime? DateCreated { get; set; }

        [Display(Name = "Updated On")]
        public DateTime? LastUpdate { get; set; }

        public int TriggerValueId { get; set; }
        public string? ScreenName { get; set; }
        public string? TriggerValueName { get; set; }
    }
}
