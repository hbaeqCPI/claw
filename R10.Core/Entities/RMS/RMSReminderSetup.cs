using R10.Core.Entities.Trademark;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.RMS
{
    public class RMSReminderSetup : RMSReminderSetupDetail
    {
        public TmkCountry? TmkCountry { get; set; }
        public TmkCaseType? TmkCaseType { get; set; }
        public List<RMSReminderSetupDoc>? RMSReminderSetupDocs { get; set; }
    }

    public class RMSReminderSetupDetail : BaseEntity
    {
        [Key]
        public int SetupId { get; set; }

        [StringLength(5)]
        public string? Country { get; set; }

        [StringLength(3)]
        [Display(Name = "Case Type")]
        public string? CaseType { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Action Type")]
        public string ActionType { get; set; }

        [StringLength(50)]
        [Display(Name = "Action Due")]
        public string? ActionDue { get; set; } = "";

        [Display(Name = "Forward to Agent")]
        public bool? ForwardToAgent { get; set; }

        [Display(Name = "POA Requirement")]
        public bool? POARequirement { get; set; }
    }
}
