using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace R10.Core.DTOs
{
    [Keyless]
    public class DocReviewDTO
    {
        public string? DocId { get; set; }

        [Display(Name = "System")]
        public string? SystemName { get; set; }

        [Display(Name = "Screen")]
        public string? Screen { get; set; }
        [Display(Name = "Source")]

        public string? CaseNumber { get; set; }

        [Display(Name = "Country")]
        public string? Country { get; set; }

        [Display(Name = "Sub Case")]
        public string? SubCase { get; set; }

        public string? Source { get; set; }
        [Display(Name = "File")]
        public string? FileName { get; set; }

        [Display(Name = "Date Uploaded")]
        public DateTime? DateUploaded { get; set; }

        [Display(Name = "Uploaded By")]
        public string? UploadedBy { get; set; }

        [Display(Name = "Reviewed?")]
        public bool SignatureReviewed { get; set; }

        public string? DocLibrary { get; set; }
        public string? DriveItemId { get; set; }
        public int? FileId { get; set; }

        public string? SystemTypeCode { get; set; }
        public string? ScreenCode { get; set; }

        [Display(Name = "Reviewed By")]
        public string? SignatureReviewedBy { get; set; }

        [Display(Name = "On")]
        public DateTime? SignatureReviewedDate { get; set; }

        public int? QESetupId { get; set; }
        public string? RoleLink { get; set; }
        public int ParentId { get; set; }

        [Display(Name = "Email Body Template")]
        public string? SignatureQETemplateName { get; set; }
        
        public string? EnvelopeId { get; set; }
        public bool SentToDocuSign { get; set; }
        public bool? SignatureCompleted { get; set; }

        [Display(Name = "Signed File Name")]
        public string? SignedFileName { get; set; }
        public string? SignedDocDriveItemId { get; set; }
        public string? DocumentCode { get; set; }
        public int? DocLogId { get; set; }
        public string? SignerRole { get; set; }
        public string? SignerName { get; set; }
        public string? SignerEmail { get; set; }
        public string? SignerAnchorCode { get; set; }
        public string? DocFileName { get; set; }
        public string? SignedDocFileName { get; set; }
        public string? DataKey { get; set; }
        public int? SignedDocLogId { get; set; }
        public string? EnvelopeStatus { get; set; }
        

        [NotMapped]
        public string? Name { get; set; }
        [NotMapped]
        public string? Id { get; set; }
        [NotMapped]
        public string? DocLibraryFolder { get; set; }

        [NotMapped]
        [Display(Name = "Source")]
        public string? SourceDescription { get; set; }

        [NotMapped]
        public string? LogFile { get; set; }
        [NotMapped]
        public string? ItemId { get; set; }
        [NotMapped]
        public string? Document { get; set; }
        [NotMapped]
        public int? SignatureQESetupId { get; set; }
        [NotMapped]
        public int RecKey { get; set; }
        [NotMapped]
        public string? SystemType { get; set; }
        [NotMapped]
        public string? SystemTypeName { get; set; }
        [NotMapped]
        public string? DocName { get; set; }
        [NotMapped]
        public bool Successful { get; set; }
    }

    public class DocReviewUpdateDTO
    {
        public int RecId { get; set; }
        public bool SignatureReviewed { get; set; }
    }
}
