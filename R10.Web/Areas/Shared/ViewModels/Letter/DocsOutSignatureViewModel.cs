
using DocumentFormat.OpenXml.Office.CoverPageProps;
using DocumentFormat.OpenXml.Wordprocessing;
using R10.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class DocsOutSignatureViewModel
    {
        public int QESetupId { get; set; }
        public int ParentId { get; set; }
        public DocsOutSignatureDocViewModel UserFile { get; set; }
        public string? ScreenCode { get; set; }
        public string? SystemTypeCode { get; set; }
        public string? RoleLink { get; set; }
        public int DocLogId { get; set; }
        public string? DocumentCode { get; set; }
        public string? SharePointDocLibrary { get; set; }
        public DocuSignRecipientParam Signer { get; set; }
        public List<DocuSignAnchorTab> SignHereTabs { get; set; }
        public List<DocuSignAnchorTab> InitialHereTabs { get; set; }
        public List<DocuSignAnchorTab> DateSignedTabs { get; set; }
    }

    public class DocsOutSignatureDocViewModel
    {
        public string? Name { get; set; }
        public string? FileName { get; set; }
        public int? FileId { get; set; }
        public string? StrId { get; set; }
    }

    public class DocsOutSignatureSignedViewModel
    {
        public int DocLogId { get; set; }
        public int ParentId { get; set; }
        public string? EnvelopeId { get; set; }
        public string? LetFile { get; set; }
        public string? ScreenCode { get; set; }
        public string? SystemTypeCode { get; set; }
        public string? DocumentCode { get; set; }
    }

   

}
