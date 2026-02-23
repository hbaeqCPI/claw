using R10.Core.Entities.Documents;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static R10.Web.Helpers.ImageHelper;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class DocDocumentViewModel : DocDocument         //DocDocumentDetail
    {
        public string? DocTypeName { get; set; }
        public string? DocFileName { get; set; }
        public string? UserFileName { get; set; }
        public string? ThumbFileName { get; set; }
        public int? FileSize { get; set; }

        public bool? IsImage { get; set; }
        public IEnumerable<IFormFile>? UploadedFiles { get; set; }

        public bool IsDocViewable { get; set; } = false;       // is file viewable by document viewer?
        public bool IsDocLinkable { get; set; } = false;       // is document linkable?
        public bool HasDefault { get; set; } = false;       // has default image on main screen

        // for Azure Storage
        public string? SystemType { get; set; }
        public string? ScreenCode { get; set; }
        public int ParentId { get; set; }

        public string? DocViewer { get; set; }

        [Display(Name = "Release Lock?")]
        public bool ReleaseFileLock { get; set; }

        //add for multiple file upload
        public IFormFile? UploadedFile { get; set; }

        public string DocumentLink { get; set; }
        public string? RoleLink { get; set; }
        public string SaveAction { get; set; } = "SaveDocument";

        //eSignature
        public bool? ForSignature { get; set; }
        public string? DataKey { get; set; }
        public bool? SignedDoc { get; set; }

        //External storage doc id
        public string? DriveItemId { get; set; }

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
        [NotMapped]
        public CPiSavedFileType? ViewFileType { get ; set; }
    }

    public class DocPickListViewModel {
        public string? DocName { get; set; }
    }
}
