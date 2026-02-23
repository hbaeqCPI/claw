using R10.Core.Entities.Patent;
using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Documents
{
    public class DocDocument : DocDocumentDetail
    {
        public DocType? DocType { get; set; }
        public DocFile? DocFile { get; set; }
        public DocFolder? DocFolder { get; set; }
        public List<DocDocumentTag>? DocDocumentTags { get; set; }
        public List<DocVerification>? DocVerifications { get; set; }        
        public List<PatEPODocumentCombined>? PatEPODocumentCombineds { get; set; }
        public List<DocResponsibleDocketing>? DocResponsibleDocketings { get; set; }
        public List<DocResponsibleReporting>? DocResponsibleReportings { get; set; }
        public List<DocResponsibleLog>? DocResponsibleLogs { get; set; }
        public List<DocQuickEmailLog>? DocQuickEmailLogs { get; set; }
    }

    public class DocDocumentDetail : BaseEntity
    {
        [Key]
        public int DocId { get; set; }

        public int FolderId { get; set; }

        [Display(Name = "Author")]
        public string? Author { get; set; }

        [Display(Name = "Document Name")]
        [Required]
        public string? DocName { get; set; }

        [Display(Name = "Document Type")]
        public int? DocTypeId { get; set; }

        [Display(Name = "URL")]
        public string? DocUrl { get; set; }

        public int? FileId { get; set; }


        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }

        [Display(Name = "Private?")]
        public bool IsPrivate { get; set; } = false;

        [Display(Name = "Default?")]
        public bool IsDefault { get; set; } = false;

        [Display(Name = "Print on Reports?")]
        public bool IsPrintOnReport { get; set; } = false;
        
        public string? LockedBy { get; set; }

        [Display(Name = "Tags")]
        public string? Tags { get; set; }

        [Display(Name = "Document Reviewed?")]
        public bool IsVerified { get; set; } = false;

        [Display(Name = "Include in Workflow?")]
        public bool IncludeInWorkflow { get; set; } = false;


        [Display(Name = "Docket Required?")]
        public bool IsActRequired { get; set; } = false;

        public string? Source { get; set; }

        [Display(Name = "Check Docket?")]
        public bool CheckAct { get; set; } = false;
        
        [Display(Name = "Forward document to client?")]
        public bool SendToClient { get; set; } = false;

        public DateTime? ActRequiredLastUpdate { get; set; }
    }

    public static class DocumentSourceType
    {
        // values here are tied to Source in tblDocDocument
        public const string Manual = "Manual Upload";
        public const string CPIMail = "CPI Mailbox";
        public const string EPOMail = "EPO Mailbox";
        public const string EPOOPS = "EPO";
    }
}
