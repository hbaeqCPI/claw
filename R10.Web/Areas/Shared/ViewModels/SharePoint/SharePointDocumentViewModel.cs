using DocumentFormat.OpenXml.Wordprocessing;
using R10.Core.Entities.Documents;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class SharePointDocumentViewModel
    {
        [Display(Name = "Folder")]
        public string? Folder { get; set; }
        public string? Id { get; set; }
        public string? Title { get; set; }
       
        [Display(Name = "Name")]
        public string Name { get; set; }

        [Display(Name = "Created On")]
        public DateTime? DateCreated { get; set; }

        [Display(Name = "Last Modified")]
        public DateTime? DateModified { get; set; }

        public DateTimeOffset? DateCreated_Offset { get; set; }
        public DateTimeOffset? DateModified_Offset { get; set; }

        [Display(Name = "Created By")]
        public string? CreatedBy { get; set; }

        [Display(Name = "Modified By")]
        public string? ModifiedBy { get; set; }

        public string? ViewUrl { get; set; }
        public string? EditUrl { get; set; }
        public bool? IsCheckedOut { get; set; }
        
        [Display(Name = "Checked Out By")]
        public string? CheckOutUser { get; set; }

        public string? DownloadUrl { get; set; }
        public string? ServerRelativeUrl { get; set; }

        [Display(Name = "Image")]
        public string? PreviewUrl { get; set; }
        public string? ThumbnailUrl { get; set; }

        public string? IconClass { get; set; } = "fal fa-file";
        public string? DocLibrary { get; set; }
        public string? DocLibraryFolder { get; set; }
        public string? RecKey { get; set; }
        public Dictionary<string,object>? ListItemFields { get; set; }
        public bool? IsImage { get; set; }
        public string? ListItemId { get; set; }
        public bool IsPrivate { get; set; }

        //for DocuSign
        public bool? ForSignature { get; set; }
        public string? EnvelopeId { get; set; }
        public bool? SignatureCompleted { get; set; }
        public bool SignatureReviewed { get; set; }

        [Display(Name = "Sent for eSignature?")]
        public bool SentToDocuSign { get; set; }
        public string? DataKey { get; set; }
        public bool? SignedDoc { get; set; }
        public int? QESetupId { get; set; }
        public string? RoleLink { get; set; }
        public string? SystemTypeCode { get; set; }
        public int ParentId { get; set; }
        public string? ScreenCode { get; set; }

        //MyEPO Communication
        public string? CommunicationId { get; set; }
    }

    public class SharePointDocumentEntryViewModel {
        public string? DocLibrary { get; set; }
        public string? DocLibraryFolder { get; set; }
        public string? RecKey { get; set; }
        public string? RoleLink { get; set; }
        public int ParentId { get; set; }
        public string? FolderId { get; set; }
        public string? DriveItemId { get; set; }
        public string? ListItemId { get; set; }
        public IEnumerable<IFormFile>? UploadedFiles { get; set; }
        public string? Title { get; set; }
        public string? FileName { get; set; } = "";

        [Display(Name = "Print on Reports?")]
        public bool IsPrintOnReport { get; set; }

        [Display(Name = "Default?")]
        public bool IsDefault { get; set; }
        public bool IsDefaultPrev { get; set; }

        [Display(Name = "Private?")]
        public bool IsPrivate { get; set; }

        [Display(Name = "Document Reviewed?")]
        public bool IsVerified { get; set; }

        [Display(Name = "Tags")]
        public string? Tags { get; set; }

        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }
        public bool HasDefault { get; set; }
        public bool IsImage { get; set; }

        [Display(Name = "Include in Workflow?")]
        public bool IncludeInWorkflow { get; set; }

        public string? Source { get; set; }
        public int DocId { get; set; }
        public string? Author { get; set; }
        public string? CreatedBy { get; set; }
        [Display(Name = "Document Name")]
        public string? DocName { get; set; }
        public string? OrigDocName { get; set; }
        [Display(Name = "URL")]
        public string? DocUrl { get; set; }
        public string? OrigDocUrl { get; set; }

        [Display(Name = "Type")]
        public string? Type { get; set; } = "File";

        //Document Verification
        [NotMapped]
        [Display(Name = "Corresponding Docket(s)")]
        public string? VerificationActionList { get; set; }

        [Display(Name = "Docket Required?")]
        public bool IsActRequired { get; set; }

        [NotMapped]
        [Display(Name = "Responsible (Docketing)")]
        public string[]? RespDocketings { get; set; }
        [NotMapped]
        public List<string>? DefaultRespDocketings { get; set; }

        
        [NotMapped]
        [Display(Name = "Responsible (Reporting)")]
        public string[]? RespReportings { get; set; }
        [NotMapped]
        public List<string>? DefaultRespReportings { get; set; }


        [Display(Name = "Check Docket?")]
        public bool CheckAct { get; set; }

        [Display(Name = "Forward document to client?")]
        public bool SendToClient { get; set; }


        [NotMapped]
        public string? RandomGuid { get; set; }
        [NotMapped]
        public string? ViewFilePath { get ; set; }

        public int? FileId { get; set; }
    }

    public class SharePointDocumentDownloadViewModel
    {
        public string? DriveItemId { get; set; }
        public string? Name { get; set; }
        public byte[]? FileBytes { get; set; }

    }

    public class SharePointFolderViewModel
    {
        public string? Folder { get; set; }
        public string? RecKey { get; set; }

    }

    public class SharePointVerificationViewModel
    {
        [Display(Name = "Responsible")]
        public string? Responsibles { get; set; }
        public string? CaseNumber { get; set; }
        public string? Country { get; set; }
        public string? SubCase { get; set; }
    }

    public class SharePointStorageViewModel
    {
        public string DocLibrary { get; set; }
        public string DocLibraryFolder { get; set; }
        public int ParentId { get; set; }
        public string DocName { get; set; }
        public string FileName { get; set; }
        //public byte[] Buffer { get; set; }
        public int Id { get; set; }
    }

    public class SharePointSyncToDocViewModel {
        public string? DocLibrary { get; set; }
        public string? DocLibraryFolder { get; set; }
        public string? DriveItemId { get; set; }
        public int ParentId { get; set; }
        public string? FileName { get; set; }
        public string? CreatedBy { get; set; }

        public string? Remarks { get; set; }
        public string? Tags { get; set; }
        public bool IsImage { get; set; }
        public bool IsPrivate { get; set; }
        public bool IsDefault { get; set; }
        public bool IsPrintOnReport { get; set; }
        public bool IsVerified { get; set; }
        public bool IncludeInWorkflow { get; set; }
        public bool IsActRequired { get; set; }
        public string? Source { get; set; }
        public string? Author { get; set; }

        public bool ProcessAI { get; set; } = true;

        public bool CheckAct { get; set; }

        public bool SendToClient { get; set; }

        public int? FileId { get; set; }

    }



}
