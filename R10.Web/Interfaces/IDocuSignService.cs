using DocuSign.eSign.Api;
using DocuSign.eSign.Client;
using DocuSign.eSign.Model;
using R10.Core.Entities.Documents;
using R10.Core.Queries.Shared;
using R10.Web.Areas.Shared.ViewModels;
using static DocuSign.eSign.Client.Auth.OAuth;
using static DocuSign.eSign.Client.Auth.OAuth.UserInfo;

namespace R10.Web.Interfaces
{
    public interface IDocuSignService
    {
        OAuthToken AuthenticateWithJWT(string api, string clientId, string impersonatedUserId, string authServer, byte[] privateKeyBytes);
        string SendEnvelopeViaEmail(DocuSignEnvelopeEmailParam envelopeParam);
        Stream GetSignedDocuments(DocuSignEnvelopeGetParam envelopeParam);
        Task<Envelope> GetEnvelopeData(DocuSignEnvelopeGetParam envelopeParam);
        Task<DocuSignEnvelopeEmailParam> BuildEnvelopeData(DocuSignEnvelopeBuildDataParam param);
        Task<DocuSign.eSign.Model.Recipients> GetRecipients(DocuSignEnvelopeGetParam envelopeParam);
        Dictionary<string,string>  GetDocuSignAccessToken();
        Task<EnvelopeUpdateSummary> VoidEnvelope(DocuSignEnvelopeGetParam envelopeParam, string? voidReason);
        Task AddRecipients(DocuSignEnvelopeEmailParam envelopeParam, string envelopeId);
        Task<bool> ProcessSignedDocumentsAndSave(DocDocumentListViewModel viewModelParam, string accessToken);
        Task<bool> ProcessSignedDocumentsAndSaveToSharePoint(DocDocumentListViewModel viewModelParam, string accessToken, bool needGraphClient = false);
        Task<bool> ProcessSignedDocsOutAndSaveToSharePoint(DocsOutSignatureSignedViewModel viewModelParam, string accessToken, bool needGraphClient = false);
        Task<bool> ProcessSignedDocsOutAndSave(DocsOutSignatureSignedViewModel viewModelParam, string accessToken);
        Task<List<DocuSignRecipientViewModel>> GetRecipientsForDisplay(int qeSetupId, string roleLink, string sendAs);
        Task<List<DocFileSignatureRecipient>> GetDocuSignRecipients(string envelopeId);
    }

    
}
