using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace R10.Core.Entities.Patent
{
    public class PatWorkflow : BaseEntity
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
        public string? TriggerValueName { get; set; }

        public string? ClientFilter { get; set; }

        [Display(Name = "Attorney")]
        public string? AttorneyFilter { get; set; }
        [Display(Name = "Country")]
        public string? CountryFilter { get; set; }
        [Display(Name = "Case Type")]
        public string? CaseTypeFilter { get; set; }
        [Display(Name = "Responsible Office")]
        public string? RespOfficeFilter { get; set; }

        [Display(Name = "Screen Name")]
        public int? ScreenId { get; set; }
        
        public bool? CPIWorkflow { get; set; }

        [NotMapped]
        public int[]? ClientFilterList { get; set; }
        [NotMapped]
        public int[]? AttorneyFilterList { get; set; }
        [NotMapped]
        public string[]? CountryFilterList { get; set; }
        [NotMapped]
        public string[]? CaseTypeFilterList { get; set; }
        [NotMapped]
        public string[]? RespOfficeFilterList { get; set; }

        public List<PatWorkflowAction>? WorkflowActions { get; set; }
        public SystemScreen? SystemScreen { get; set; }
        public List<PatWorkflowActionParameter>? WorkflowActionParameters { get; set; }

        [NotMapped]
        public string? CopyOptions { get; set; }
    }

    public enum PatWorkflowTriggerType
    {
        [Display(Name = "Status Changed")]
        StatusChanged,

        [Display(Name = "New Action")]
        NewAction,

        [Display(Name = "Action Closed")]
        ActionClosed,

        [Display(Name = "New File Uploaded")]
        NewFileUploaded,

        [Display(Name = "Dedocket Instruction")]
        DedocketInstruction,

        [Display(Name = "Record Deleted")]
        RecordDeleted,

        [Display(Name = "New Cost Record")]
        NewCostRecord,

        [Display(Name = "Attorney Modified")]
        AttorneyModified,

        [Display(Name = "US Related Case")]
        USRelatedCase,

        [Display(Name = "Action Delegated")]
        ActionDelegated,

        [Display(Name = "Delegated Action Completed")]
        ActionDelegatedCompleted,

        [Display(Name = "Delegated Action Reassigned")]
        ActionDelegatedReAssigned,

        [Display(Name = "Delegated Action Due date Changed")]
        ActionDelegatedDuedateChanged,

        [Display(Name = "Delegated Action Deleted")]
        ActionDelegatedDeleted,

        [Display(Name = "Email Sent")]
        EmailSent,

        [Display(Name = "Inventor Remuneration")]
        InventorRemuneration,

        [Display(Name = "Disclosure Status Changed")]
        DisclosureStatusChanged,

        [Display(Name = "Inventor Award Generated")]
        InventorAwardGenerated,

        [Display(Name = "Inventor Award Paid")]
        InventorAwardPaid,

        [Display(Name = "Document Responsible (Docketing) Assigned")]
        DocumentRespDocketingAssigned,

        [Display(Name = "Document Responsible (Docketing) Reassigned")]
        DocumentRespDocketingReAssigned,

        [Display(Name = "Indicator")]
        Indicator,

        [Display(Name = "Dedocket Instruction Completed")]
        DedocketInstructionCompleted,

        [Display(Name = "New EPO File Downloaded")]
        NewEPOFileDownloaded,

        [Display(Name = "Document Responsible (Reporting) Assigned")]
        DocumentRespReportingAssigned,

        [Display(Name = "Document Responsible (Reporting) Reassigned")]
        DocumentRespReportingReAssigned,

        [Display(Name = "Request Docket")]
        RequestDocket,
    }

}

