using DocuSign.eSign.Model;
using R10.Core.Entities;
using R10.Web.Models;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace R10.Web.Areas.Shared.ViewModels
{

    public class DocuSignEnvelopeBuildDataParam {
        public int QESetupId { get; set; }
        public int ParentId { get; set; }
        public string? ScreenCode { get; set; }
        public string? SystemTypeCode { get; set; }
        public string? RoleLink { get; set; }
        public string? DocToSignInBase64String { get; set; }
        public string? FileName { get; set; }
        public string? FileExtension { get; set; }
        public ClaimsPrincipal ClaimsPrincipal { get; set; }
        public List<DocuSignRecipientParam> Signers { get; set; }
    }

    public class DocuSignEnvelopeEmailParam
    {
        public string? AuthServer { get; set; }
        public List<DocuSignRecipientParam> Signers { get; set; }
        public List<DocuSignRecipientParam> CcRecipients { get; set; }
        public string? AccessToken { get; set; }
        public string? BasePath { get; set; }
        public string? AccountId { get; set; }
        public string? EnvelopeStatus { get; set; } = "sent";
        public List<DocuSignDocumentParam> DocumentsToSign { get; set; }
        public string? EmailSubject { get; set; }
        public string? EmailBody { get; set; }
        public List<DocuSignAnchorTab> SignHereTabs { get; set; }
        public List<DocuSignAnchorTab> InitialHereTabs { get; set; }
        public List<DocuSignAnchorTab> DateSignedTabs { get; set; }
    }

    public class DocuSignDocumentParam
    {
        public string? DocToSignInBase64String { get; set; }
        public string? Name { get; set; }
        public string? Extension { get; set; }
    }

    public class DocuSignRecipientParam
    {
        public string? Email { get; set; }
        public string? Name { get; set; }
        public int RecipientId { get; set; }
        public int RoutingOrder { get; set; } = 1;
        public string? AnchorCode { get; set; } = "Default";
    }

    public class DocuSignEnvelopeGetParam
    {
        public string? AuthServer { get; set; }
        public string? AccessToken { get; set; }
        public string? BasePath { get; set; }
        public string? AccountId { get; set; }
        public string? EnvelopeId { get; set; }
    }

    public class DocuSignEmailViewModel
    {

        public string? RoleLink { get; set; }
        public List<DocuSignRecipientParam> To { get; set; }
        public List<DocuSignRecipientParam> CopyTo { get; set; }
        public string? Subject { get; set; }
        public string? Body { get; set; }

    }

    public class DocuSignAnchorParam
    {
        public string AnchorCode { get; set; }
        public List<SignHere> SignHeres { get; set; }
        public List<InitialHere> InitialHeres { get; set; }
        public List<DateSigned> DateSigneds { get; set; }
    }

    public class DocuSignRecipientViewModel
    {
        [Display(Name = "Role")]
        public string? Role { get; set; }
        [Display(Name = "Name")]
        public string? Name { get; set; }
        [Display(Name = "Email")]
        public string? Email { get; set; }

        public int QESetupId { get; set; }
        public string? RoleLink { get; set; }
        public string? Source { get; set; }

        [Display(Name = "Sent On")]
        public DateTime? sentDateTime { get; set; }

        [Display(Name = "Signed On")]
        public DateTime? signedDateTime { get; set; }

        public string? envelopeId { get; set; }
    }


    
}
