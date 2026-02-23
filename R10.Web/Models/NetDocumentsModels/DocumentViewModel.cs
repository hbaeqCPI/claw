using R10.Core.Entities.Documents;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Web.Models.NetDocumentsModels
{
    public class DocumentViewModel : DocDocument
    {
        //NetDocs Document
        public string? Id { get; set; }
        public string? Name { get; set; }
        //public string? Type { get; set; }
        public string? Extension { get; set; }
        public int Size { get; set; }
        public int Version { get; set; }
        public string? LastUser { get; set; }
        public DateTime? EditDate { get; set; }
        public DateTime? CreateDate { get; set; }

        //NetDocs Viewer (Grid/Gallery)
        public string? Title { get; set; }
        public string? ContainerId { get; set; }
        public string? ContainerName { get; set; }
        public string? IconClass { get; set; }
        public bool IsImage { get; set; }
        public string? ImageUrl { get; set; }   //Gallery view image source
        public string? WorkUrl {  get; set; }   //NetDocs document url (vault.netvoyage.com)

        //NetDocs Editor
        public IFormFile? FormFile { get; set; }
        public string? DocumentName { get; set; }
        //public string? WorkType { get; set; }

        //DocFile
        public string? DriveItemId { get; set; }
        public bool? ForSignature { get; set; }
        public bool? SignedDoc { get; set; }

        //eSignature
        public string? DataKey { get; set; }

        //Document Verification
        [NotMapped]
        [Display(Name = "Corresponding Action(s)")]
        public string[]? VerificationActionList { get; set; }
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
        [NotMapped]
        public string? RandomGuid { get; set; }
        [NotMapped]
        public string? ViewFilePath { get ; set; }


        //RMS and Foreign Filing
        public int DueId { get; set; }
        public int RequiredDocId { get; set; }
    }
}
