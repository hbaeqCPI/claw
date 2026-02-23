using R10.Core.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class DocDocumentListViewModel : BaseEntity
    {
        public int DocId { get; set; }

        public int FolderId { get; set; }

        public int FileId { get; set; }
        
        public string? DocFileName { get; set; }

        public string? DocUrl { get; set; }

        [Display(Name = "Author")]
        public string? Author { get; set; }

        [Display(Name = "Document Name")]
        public string? DocName { get; set; }

        [Display(Name = "Type")]
        public string? DocTypeName { get; set; }

        [Display(Name = "Private?")]
        public bool IsPrivate { get; set; } = false;

        public string? IconClass { get; set; }

        //[NotMapped]
        public bool IsDocViewable { get; set; } = false;       // is file viewable by document viewer?

        //[NotMapped]
        public bool IsDocLinkable { get; set; } = false;       // is document linkable?

        public string? ThumbFileName { get; set; }
        public string? LockedBy { get; set; }
        public bool FolderIsPublic { get; set; }
        public string? FolderCreatedBy { get; set; }

        [Display(Name = "Folder")]
        public string? FolderName { get; set; }
        
        [Display(Name = "User File Name")]
        public string? UserFileName { get; set; }

        public List<string>? Tags { get; set; }

        // for Azure Storage
        public string? SystemType { get; set; }
        public string? ScreenCode { get; set; }
        public int ParentId { get; set; }
        public byte[]? FileBytes { get; set; }

        //for DocuSign
        public bool? ForSignature { get; set; }
        public string? EnvelopeId { get; set; }
        [Display(Name = "Completed?")]
        public bool? SignatureCompleted { get; set; }
        [Display(Name = "Sent for eSignature?")]
        public bool SentToDocuSign { get; set; }
        public string? DataKey { get; set; }
        public bool? SignedDoc { get; set; }
        public int? QESetupId { get; set; }
        public string? RoleLink { get; set; }
        public string? DocLibrary { get; set; }
        public string? DocLibraryFolder { get; set; }
        //public string? RecKey { get; set; }
        public string? Id { get; set; }
        public string? SystemTypeCode { get; set; }
        public bool? SignatureReviewed { get; set; }

        //Document Verification
        public bool IsDocVerificationLinked { get; set; } = false;
        public DateTime? UploadedDate { get; set; }
        public string? Source { get; set; }
        public string? Responsibles { get; set; }
        
        public string? CaseNumber { get; set; }
        public string? Country { get; set; }
        public string? SubCase { get; set; }

        //MyEPO Communication
        public string? CommunicationId { get; set; }
    }
}
