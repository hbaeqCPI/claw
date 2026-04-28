using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LawPortal.Core.DTOs
{
    [Keyless]
    public class DocumentVerificationCommunicationDTO
    {
        public int? DocId { get; set; }

        public string? System { get; set; }

        [Display(Name = "Document Name")]
        public string? DocName { get; set; }

        public string? DocFileName { get; set; }

        public int? ParentId { get; set; }

        [Display(Name = "Uploaded Date")]
        public DateTime? UploadedDate { get; set; }       
       
        public string? CaseNumber { get; set; }

        [Display(Name = "Country")]
        public string? Country { get; set; }

        [Display(Name = "Sub Case")]
        public string? SubCase { get; set; }

        public string? RespOffice { get; set; }

        public string? RespDocketing { get; set; }

        public string? RespReporting { get; set; }

        public string? DriveItemId { get; set; }

        public string? DocLibrary { get; set; }

        [NotMapped]
        public bool CanUploadDocument { get; set; }
    }
}
