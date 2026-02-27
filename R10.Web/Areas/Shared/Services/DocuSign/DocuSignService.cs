using Azure.Core;
using DocuSign.eSign.Api;
using DocuSign.eSign.Client;
using DocuSign.eSign.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.IdentityModel.Tokens;
using MsgKit;
using Newtonsoft.Json;
using Org.BouncyCastle.Cms;
using Org.BouncyCastle.Crypto;
using R10.Core.DTOs;
using R10.Core.Entities.Documents;
using R10.Core.Entities.Shared;
using R10.Core.Helpers;
using R10.Core.Interfaces;
using R10.Core.Queries.Shared;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;
using R10.Web.Helpers;
using R10.Web.Interfaces;
using R10.Web.Models;
using R10.Web.Services.DocumentStorage;
using R10.Web.Services.SharePoint;
using System.Diagnostics.Metrics;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using static DocuSign.eSign.Client.Auth.OAuth;
using static DocuSign.eSign.Client.Auth.OAuth.UserInfo;

namespace R10.Web.Services
{
    public class DocuSignService : IDocuSignService
    {
        private readonly IQuickEmailRepository _quickEmailRepository;
        private readonly DocuSignSettings _docuSignSettings;
        private readonly ISystemSettings<DefaultSetting> _settings;
        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _hostingEnvironment;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly IDocumentService _docService;
        protected readonly ClaimsPrincipal _user;
        private readonly IDocumentsViewModelService _docViewModelService;
        private readonly ISharePointService _sharePointService;
        private readonly IApplicationDbContext _repository;
        private readonly GraphSettings _graphSettings;
        private readonly IHttpContextAccessor _httpContextAccessor;
        protected readonly IDocumentStorage _documentStorage;

        private static readonly string DocuSignFolder = @"Resources\DocuSign";

        public DocuSignService(IQuickEmailRepository quickEmailRepository,
                               IOptions<DocuSignSettings> docuSignSettings, ISystemSettings<DefaultSetting> settings,
                               Microsoft.AspNetCore.Hosting.IHostingEnvironment hostingEnvironment, IStringLocalizer<SharedResource> localizer, IDocumentService docService,
                               ClaimsPrincipal user, IDocumentsViewModelService docViewModelService,
                               ISharePointService sharePointService, IApplicationDbContext repository, IOptions<GraphSettings> graphSettings, IHttpContextAccessor httpContextAccessor, IDocumentStorage documentStorage)
        {
            _quickEmailRepository = quickEmailRepository;
            _docuSignSettings = docuSignSettings.Value;
            _settings = settings;
            _hostingEnvironment = hostingEnvironment;
            _localizer = localizer;
            _docService = docService;
            _user = user;
            _docViewModelService = docViewModelService;
            _sharePointService = sharePointService;
            _repository = repository;
            _graphSettings = graphSettings.Value;
            _httpContextAccessor = httpContextAccessor;
            _documentStorage = documentStorage;
        }

        public OAuthToken AuthenticateWithJWT(string api, string clientId, string impersonatedUserId, string authServer, byte[] privateKeyBytes)
        {
            var docuSignClient = new DocuSignClient();
            var apiType = Enum.Parse<DocuSignAPIType>(api);
            var scopes = new List<string>
                {
                    "signature",
                    "impersonation",
                };
            if (apiType == DocuSignAPIType.Rooms)
            {
                scopes.AddRange(new List<string>
                {
                    "dtr.rooms.read",
                    "dtr.rooms.write",
                    "dtr.documents.read",
                    "dtr.documents.write",
                    "dtr.profile.read",
                    "dtr.profile.write",
                    "dtr.company.read",
                    "dtr.company.write",
                    "room_forms",
                });
            }

            if (apiType == DocuSignAPIType.Click)
            {
                scopes.AddRange(new List<string>
                {
                    "click.manage",
                    "click.send",
                });
            }

            if (apiType == DocuSignAPIType.Monitor)
            {
                scopes.AddRange(new List<string>
                {
                    "signature",
                    "impersonation",
                });
            }

            if (apiType == DocuSignAPIType.Admin)
            {
                scopes.AddRange(new List<string>
                {
                    "user_read",
                    "user_write",
                    "account_read",
                    "organization_read",
                    "group_read",
                    "permission_read",
                    "identity_provider_read",
                    "domain_read",
                    "user_data_redact",
            });
            }

            return docuSignClient.RequestJWTUserToken(
                clientId,
                impersonatedUserId,
                authServer,
                privateKeyBytes,
                1,
                scopes);
        }

        public string SendEnvelopeViaEmail(DocuSignEnvelopeEmailParam envelopeParam)
        {
            var docuSignClientMain = new DocuSignClient();
            docuSignClientMain.SetOAuthBasePath(envelopeParam.AuthServer);
            DocuSign.eSign.Client.Auth.OAuth.UserInfo userInfo = docuSignClientMain.GetUserInfo(envelopeParam.AccessToken);
            Account acct = userInfo.Accounts.FirstOrDefault();

            if (acct != null)
            {
                envelopeParam.BasePath = acct.BaseUri + "/restapi";
                envelopeParam.AccountId = acct.AccountId;

                EnvelopeDefinition env = MakeEnvelope(envelopeParam);
                var docuSignClient = new DocuSignClient(envelopeParam.BasePath);
                docuSignClient.Configuration.DefaultHeader.Add("Authorization", "Bearer " + envelopeParam.AccessToken);

                EnvelopesApi envelopesApi = new EnvelopesApi(docuSignClient);
                EnvelopeSummary results = envelopesApi.CreateEnvelope(envelopeParam.AccountId, env);

                //Push envelopeId to DocuSign listener
                SendToListener(results.EnvelopeId);

                return results.EnvelopeId;
            }
            return string.Empty;
        }

        public Stream GetSignedDocuments(DocuSignEnvelopeGetParam envelopeParam)
        {

            var docuSignClientMain = new DocuSignClient();
            docuSignClientMain.SetOAuthBasePath(envelopeParam.AuthServer);
            DocuSign.eSign.Client.Auth.OAuth.UserInfo userInfo = docuSignClientMain.GetUserInfo(envelopeParam.AccessToken);
            Account acct = userInfo.Accounts.FirstOrDefault();

            if (acct != null)
            {
                envelopeParam.BasePath = acct.BaseUri + "/restapi";
                envelopeParam.AccountId = acct.AccountId;


                var docuSignClient = new DocuSignClient(envelopeParam.BasePath);
                docuSignClient.Configuration.DefaultHeader.Add("Authorization", "Bearer " + envelopeParam.AccessToken);

                EnvelopesApi envelopesApi = new EnvelopesApi(docuSignClient);

                // produce a ZIP file with all documents including the CoC
                //Stream results1 = envelopesApi.GetDocument(envelopeParam.AccountId, envelopeParam.EnvelopeId, "archive");

                // produce a PDF combining all signed documents as well as the CoC
                Stream results2 = envelopesApi.GetDocument(envelopeParam.AccountId, envelopeParam.EnvelopeId, "combined");

                // produce a particular document with documentId "1"
                //Stream results3 = envelopesApi.GetDocument(envelopeParam.AccountId, envelopeParam.EnvelopeId, "1");

                return results2;
            }
            return null;
        }

        public async Task<Envelope> GetEnvelopeData(DocuSignEnvelopeGetParam envelopeParam)
        {

            var docuSignClientMain = new DocuSignClient();
            docuSignClientMain.SetOAuthBasePath(envelopeParam.AuthServer);
            DocuSign.eSign.Client.Auth.OAuth.UserInfo userInfo = docuSignClientMain.GetUserInfo(envelopeParam.AccessToken);
            Account acct = userInfo.Accounts.FirstOrDefault();

            if (acct != null)
            {
                envelopeParam.BasePath = acct.BaseUri + "/restapi";
                envelopeParam.AccountId = acct.AccountId;


                var docuSignClient = new DocuSignClient(envelopeParam.BasePath);
                docuSignClient.Configuration.DefaultHeader.Add("Authorization", "Bearer " + envelopeParam.AccessToken);

                EnvelopesApi envelopesApi = new EnvelopesApi(docuSignClient);
                var result = await envelopesApi.GetEnvelopeAsync(envelopeParam.AccountId, envelopeParam.EnvelopeId);
                return result;
            }
            return null;
        }

        public async Task<DocuSignEnvelopeEmailParam> BuildEnvelopeData(DocuSignEnvelopeBuildDataParam param)
        {
            var viewModel = await GetEmailData(param.QESetupId, param.ParentId, param.ScreenCode, param.RoleLink, param.SystemTypeCode);
            if (viewModel != null && !string.IsNullOrEmpty(viewModel.Subject) && (param.Signers != null || (viewModel.To != null && viewModel.To.Any(t => !string.IsNullOrEmpty(t.Email)))))
            {
                string authServer = _docuSignSettings.AuthServer;
                var docsToSign = new List<DocuSignDocumentParam>();
                docsToSign.Add(new DocuSignDocumentParam() { DocToSignInBase64String = param.DocToSignInBase64String, Name = param.FileName, Extension = param.FileExtension });

                var signers = new List<DocuSignRecipientParam>();
                var counter = 1;

                //not from QE setup (like EFS)
                if (param.Signers != null)
                {
                    foreach (var item in param.Signers)
                    {
                        signers.Add(new DocuSignRecipientParam
                        {
                            Email = item.Email,
                            Name = item.Name,
                            RecipientId = counter++,
                            RoutingOrder = item.RoutingOrder,
                            AnchorCode = item.AnchorCode
                        });
                    }
                }
                else
                {
                    foreach (var email in viewModel.To)
                    {
                        signers.Add(new DocuSignRecipientParam
                        {
                            Email = email.Email,
                            Name = email.Name,
                            RecipientId = counter++,
                            RoutingOrder = email.RoutingOrder,
                            AnchorCode = email.AnchorCode
                        });
                    }
                }

                var copyTos = new List<DocuSignRecipientParam>();
                if (viewModel.CopyTo.Any())
                {
                    foreach (var email in viewModel.CopyTo)
                    {
                        copyTos.Add(new DocuSignRecipientParam
                        {
                            Email = email.Email,
                            Name = email.Name,
                            RecipientId = counter++,
                            RoutingOrder = 2
                        });
                    }
                }
                else
                {
                    copyTos.Add(new DocuSignRecipientParam
                    {
                        Email = param.ClaimsPrincipal.GetEmail(),
                        Name = param.ClaimsPrincipal.GetFullName(),
                        RecipientId = counter++,
                        RoutingOrder = 2
                    });
                }

                var envelopeParam = new DocuSignEnvelopeEmailParam()
                {
                    AuthServer = authServer,
                    Signers = signers,
                    CcRecipients = copyTos,
                    DocumentsToSign = docsToSign,
                    EmailSubject = viewModel.Subject,
                    EmailBody = viewModel.Body
                };
                return envelopeParam;
            }
            return null;
        }

        public async Task<DocuSign.eSign.Model.Recipients> GetRecipients(DocuSignEnvelopeGetParam envelopeParam)
        {
            var docuSignClientMain = new DocuSignClient();
            docuSignClientMain.SetOAuthBasePath(envelopeParam.AuthServer);
            DocuSign.eSign.Client.Auth.OAuth.UserInfo userInfo = docuSignClientMain.GetUserInfo(envelopeParam.AccessToken);
            Account acct = userInfo.Accounts.FirstOrDefault();

            if (acct != null)
            {
                envelopeParam.BasePath = acct.BaseUri + "/restapi";
                envelopeParam.AccountId = acct.AccountId;


                var docuSignClient = new DocuSignClient(envelopeParam.BasePath);
                docuSignClient.Configuration.DefaultHeader.Add("Authorization", "Bearer " + envelopeParam.AccessToken);

                EnvelopesApi envelopesApi = new EnvelopesApi(docuSignClient);
                var result = await envelopesApi.ListRecipientsAsync(envelopeParam.AccountId, envelopeParam.EnvelopeId);
                return result;
            }
            return null;
        }

        public async Task<List<DocuSignRecipientViewModel>> GetRecipientsForDisplay(int qeSetupId, string roleLink, string sendAs)
        {
            throw new NotImplementedException();
        }

        #region Email Data helpers
        private async Task<DocuSignEmailViewModel> GetEmailData(int qeSetupId, int parentId, string screenCode, string roleLink, string systemTypeCode)
        {
            throw new NotImplementedException();
        }

        private async Task<object> GetParentData(int parentId, string screenCode, string systemTypeCode)
        {
            object data = null;
            switch (screenCode)
            {
                case ScreenCode.Invention:
                    data = await _quickEmailRepository.GetPatInvention(parentId);
                    break;

                case ScreenCode.Application:
                    data = await _quickEmailRepository.GetPatCountryApplication(parentId);
                    break;

                case ScreenCode.Trademark:
                    data = await _quickEmailRepository.GetTmkTrademark(parentId);
                    break;

                case ScreenCode.Action:
                    switch (systemTypeCode)
                    {
                        case SystemTypeCode.Patent:
                            data = await _quickEmailRepository.GetPatActionDue(parentId);
                            break;

                        case SystemTypeCode.Trademark:
                            data = await _quickEmailRepository.GetTmkActionDue(parentId);
                            break;

                    }
                    break;
            }
            return data;
        }

        private async Task<List<DocuSignRecipientParam>> GenerateEmailAddresses(object quickEmail, string sendAs)
        {
            throw new NotImplementedException();
        }

        private string GenerateSubject(string subject, object data, string languageCulture)
        {
            if (string.IsNullOrEmpty(subject) || data == null)
                return subject;

            return ReplaceMergeFields(subject, data, languageCulture ?? "en");
        }

        private string GenerateBody(object quickEmail, object data)
        {
            throw new NotImplementedException();
        }

        private string ReplaceMergeFields(string text, object data, string languageCulture)
        {
            Type type = data.GetType();
            PropertyInfo[] properties = type.GetProperties();
            foreach (PropertyInfo property in properties)
            {
                var oldValue = "{{" + property.Name + "}}";
                if (text.Contains(oldValue))
                {
                    var newValue = "";
                    if (property.GetValue(data) != null)
                    {
                        var isDate = property.PropertyType == typeof(DateTime?) || property.PropertyType == typeof(DateTime);
                        if (!isDate)
                        {
                            var isDouble = property.PropertyType == typeof(double);
                            if (isDouble)
                                newValue = ((double)property.GetValue(data)).FormatToDisplay(languageCulture);
                            else
                            {
                                var isDecimal = property.PropertyType == typeof(decimal);
                                if (isDecimal)
                                    newValue = ((decimal)property.GetValue(data)).FormatToDisplay(languageCulture);
                                else
                                {
                                    newValue = property.GetValue(data).ToString();
                                }
                            }
                        }
                        else
                            newValue = ((DateTime?)property.GetValue(data)).FormatToDisplay(languageCulture);
                    }

                    text = text.Replace(oldValue, newValue);
                }
            }
            return text;
        }

        #endregion

        private EnvelopeDefinition MakeEnvelope(DocuSignEnvelopeEmailParam envelopeParam)
        {

            EnvelopeDefinition env = new EnvelopeDefinition();
            env.EmailSubject = envelopeParam.EmailSubject;
            env.EmailBlurb = envelopeParam.EmailBody;

            var documentId = 0;
            env.Documents = new List<Document>();
            foreach (var item in envelopeParam.DocumentsToSign)
            {
                documentId++;
                var doc = new Document
                {
                    DocumentBase64 = item.DocToSignInBase64String,
                    Name = item.Name,
                    FileExtension = item.Extension,
                    DocumentId = documentId.ToString()
                };
                env.Documents.Add(doc);
            }

            // Create signHere fields (also known as tabs) on the documents,
            // We're using anchor (autoPlace) positioning
            // The DocuSign platform searches throughout your envelope's
            // documents for matching anchor strings. 

            var anchorParams = new List<DocuSignAnchorParam>();
            //SignHere
            if (envelopeParam.SignHereTabs != null && envelopeParam.SignHereTabs.Count > 0)
            {
                var anchorCodes = envelopeParam.SignHereTabs.Select(x => x.DocuSignAnchor.AnchorCode).Distinct().ToList();
                foreach (var anchorCode in anchorCodes)
                {
                    var signHeres = new List<SignHere>();

                    foreach (var tab in envelopeParam.SignHereTabs.Where(s => s.DocuSignAnchor.AnchorCode == anchorCode).ToList())
                    {
                        signHeres.Add(new SignHere
                        {
                            AnchorString = tab.Anchor,
                            AnchorUnits = "pixels",
                            AnchorYOffset = tab.AnchorYOffSet.ToString(),
                            AnchorXOffset = tab.AnchorXOffSet.ToString()
                        });
                    }
                    anchorParams.Add(new DocuSignAnchorParam { AnchorCode = anchorCode, SignHeres = signHeres });
                }
            }

            //InitialHere
            if (envelopeParam.InitialHereTabs != null && envelopeParam.InitialHereTabs.Count > 0)
            {
                var anchorCodes = envelopeParam.InitialHereTabs.Select(x => x.DocuSignAnchor.AnchorCode).Distinct().ToList();
                foreach (var anchorCode in anchorCodes)
                {
                    var initialHeres = new List<InitialHere>();

                    foreach (var tab in envelopeParam.InitialHereTabs.Where(s => s.DocuSignAnchor.AnchorCode == anchorCode).ToList())
                    {
                        initialHeres.Add(new InitialHere
                        {
                            AnchorString = tab.Anchor,
                            AnchorUnits = "pixels",
                            AnchorYOffset = tab.AnchorYOffSet.ToString(),
                            AnchorXOffset = tab.AnchorXOffSet.ToString()
                        });
                    }
                    var param = anchorParams.FirstOrDefault(a => a.AnchorCode == anchorCode);
                    if (param != null)
                    {
                        param.InitialHeres = initialHeres;
                    }
                    else
                    {
                        anchorParams.Add(new DocuSignAnchorParam { AnchorCode = anchorCode, InitialHeres = initialHeres });
                    }
                }
            }

            //DateSigned
            if (envelopeParam.DateSignedTabs != null && envelopeParam.DateSignedTabs.Count > 0)
            {
                var anchorCodes = envelopeParam.DateSignedTabs.Select(x => x.DocuSignAnchor.AnchorCode).Distinct().ToList();
                foreach (var anchorCode in anchorCodes)
                {
                    var dateHeres = new List<DateSigned>();

                    foreach (var tab in envelopeParam.DateSignedTabs.Where(s => s.DocuSignAnchor.AnchorCode == anchorCode).ToList())
                    {
                        dateHeres.Add(new DateSigned
                        {
                            AnchorString = tab.Anchor,
                            AnchorUnits = "pixels",
                            AnchorYOffset = tab.AnchorYOffSet.ToString(),
                            AnchorXOffset = tab.AnchorXOffSet.ToString()
                        });
                    }
                    var param = anchorParams.FirstOrDefault(a => a.AnchorCode == anchorCode);
                    if (param != null)
                    {
                        param.DateSigneds = dateHeres;
                    }
                    else
                    {
                        anchorParams.Add(new DocuSignAnchorParam { AnchorCode = anchorCode, DateSigneds = dateHeres });
                    }

                }
            }

            var signers = new List<Signer>();
            foreach (var item in envelopeParam.Signers)
            {
                // Tabs are set per recipient / signer
                Tabs signerTabs = new Tabs();
                var anchorCode = item.AnchorCode ?? "Default";

                var signHereTabs = anchorParams.Where(a => a.AnchorCode == anchorCode && a.SignHeres != null).SelectMany(a => a.SignHeres).ToList();
                if (signHereTabs != null && signHereTabs.Count > 0)
                    signerTabs.SignHereTabs = signHereTabs;

                var initialHereTabs = anchorParams.Where(a => a.AnchorCode == anchorCode && a.InitialHeres != null).SelectMany(a => a.InitialHeres).ToList();
                if (initialHereTabs != null && initialHereTabs.Count > 0)
                    signerTabs.InitialHereTabs = initialHereTabs;

                var dateSignedTabs = anchorParams.Where(a => a.AnchorCode == anchorCode && a.DateSigneds != null).SelectMany(a => a.DateSigneds).ToList();
                if (dateSignedTabs != null && dateSignedTabs.Count > 0)
                    signerTabs.DateSignedTabs = dateSignedTabs;

                var signer = new Signer
                {
                    Email = item.Email,
                    Name = item.Name,
                    RecipientId = item.RecipientId.ToString(),
                    RoutingOrder = item.RoutingOrder.ToString(),
                    Tabs = signerTabs
                };
                signers.Add(signer);
            }

            // routingOrder (lower means earlier) determines the order of deliveries
            // to the recipients. Parallel routing order is supported by using the
            // same integer as the order for two or more recipients.

            var ccRecipients = new List<CarbonCopy>();
            foreach (var item in envelopeParam.CcRecipients)
            {
                var cc = new CarbonCopy
                {
                    Email = item.Email,
                    Name = item.Name,
                    RecipientId = item.RecipientId.ToString(),
                    RoutingOrder = item.RoutingOrder.ToString()
                };
                ccRecipients.Add(cc);
            }

            // Add the recipients to the envelope object
            DocuSign.eSign.Model.Recipients recipients = new DocuSign.eSign.Model.Recipients
            {
                Signers = signers,
                CarbonCopies = ccRecipients,
            };
            env.Recipients = recipients;

            // Request that the envelope be sent by setting |status| to "sent".
            // To request that the envelope be created as a draft, set to "created"
            env.Status = envelopeParam.EnvelopeStatus;
            return env;

        }

        public Dictionary<string, string> GetDocuSignAccessToken()
        {
            var privateKeyPath = Path.Combine(_hostingEnvironment.ContentRootPath, DocuSignFolder, _docuSignSettings.PrivateKeyFile);
            var privateKey = System.IO.File.ReadAllBytes(privateKeyPath);

            try
            {
                var accessToken = AuthenticateWithJWT("ESignature", _docuSignSettings.ClientId, _docuSignSettings.ImpersonatedUserId,
                                                        _docuSignSettings.AuthServer, privateKey);

                if (accessToken != null)
                {
                    return new Dictionary<string, string> { { "AccessToken", accessToken.access_token } };
                }
            }
            catch (ApiException apiException)
            {
                if (apiException.Message.Contains("consent_required"))
                {
                    // build a URL to provide consent for this Integration Key and this userId
                    string url = $"https://{_docuSignSettings.AuthServer}/oauth/auth?response_type=code&scope=impersonation%20signature" +
                                 $"&client_id={_docuSignSettings.ClientId}&redirect_uri={_docuSignSettings.DeveloperServer}";
                    return new Dictionary<string, string> { { "ConsentRequired", url } };
                }
            }
            return new Dictionary<string, string> { { "AuthFailed", _localizer["Authentication to DocuSign failed. Please check your DocuSign settings."].ToString() } };
        }

        public async Task<EnvelopeUpdateSummary> VoidEnvelope(DocuSignEnvelopeGetParam envelopeParam, string? voidReason)
        {
            var docuSignClientMain = new DocuSignClient();
            docuSignClientMain.SetOAuthBasePath(envelopeParam.AuthServer);
            DocuSign.eSign.Client.Auth.OAuth.UserInfo userInfo = docuSignClientMain.GetUserInfo(envelopeParam.AccessToken);
            Account acct = userInfo.Accounts.FirstOrDefault();

            if (acct != null)
            {
                envelopeParam.BasePath = acct.BaseUri + "/restapi";
                envelopeParam.AccountId = acct.AccountId;

                var docuSignClient = new DocuSignClient(envelopeParam.BasePath);
                docuSignClient.Configuration.DefaultHeader.Add("Authorization", "Bearer " + envelopeParam.AccessToken);

                EnvelopesApi envelopesApi = new EnvelopesApi(docuSignClient);

                var env = new Envelope();
                env.Status = "voided";
                env.VoidedReason = voidReason ?? "";
                var result = await envelopesApi.UpdateAsync(envelopeParam.AccountId, envelopeParam.EnvelopeId, env);
                return result;
            }
            return null;
        }

        public async System.Threading.Tasks.Task AddRecipients(DocuSignEnvelopeEmailParam envelopeParam, string envelopeId)
        {
            //Recipients
            var recipientList = new List<DocFileSignatureRecipient>();
            foreach (var signer in envelopeParam.Signers)
            {
                recipientList.Add(new DocFileSignatureRecipient()
                {
                    Email = signer.Email,
                    RecipientName = signer.Name,
                    RecipientId = signer.RecipientId,
                    RoutingOrder = signer.RoutingOrder,
                    EnvelopeId = envelopeId,
                    RecipientType = "signer"
                });
            }
            foreach (var ccRecipient in envelopeParam.CcRecipients)
            {
                recipientList.Add(new DocFileSignatureRecipient()
                {
                    Email = ccRecipient.Email,
                    RecipientName = ccRecipient.Name,
                    RecipientId = ccRecipient.RecipientId,
                    RoutingOrder = ccRecipient.RoutingOrder,
                    EnvelopeId = envelopeId,
                    RecipientType = "carboncopy"
                });
            }

            if (recipientList.Count > 0)
            {
                await _docService.AddSignatureRecipients(recipientList, envelopeId);
            }
        }

        public async Task<bool> ProcessSignedDocumentsAndSave(DocDocumentListViewModel viewModelParam, string accessToken)
        {
            var envelopeParam = new DocuSignEnvelopeGetParam()
            {
                AuthServer = _docuSignSettings.AuthServer,
                AccessToken = accessToken,
                EnvelopeId = viewModelParam.EnvelopeId
            };

            var envelope = await GetEnvelopeData(envelopeParam);
            if (envelope.Status.ToLower() == "completed")
            {
                var stream = GetSignedDocuments(envelopeParam);
                if (stream != null)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        stream.CopyTo(memoryStream);
                        memoryStream.Position = 0;

                        var viewModel = new DocDocumentViewModel
                        {
                            DocFileName = $"{viewModelParam.DocName}-Signed.pdf",
                            CreatedBy = _user.GetUserName(),
                            UpdatedBy = _user.GetUserName(),
                            DateCreated = DateTime.Now,
                            LastUpdate = DateTime.Now,
                            SystemType = viewModelParam.SystemType,
                            ScreenCode = viewModelParam.ScreenCode,
                            ParentId = viewModelParam.ParentId,
                            DataKey = viewModelParam.DataKey,
                            SignedDoc = true
                        };
                        await _docViewModelService.SaveDocumentFromStream(viewModel, memoryStream);
                        await _docService.MarkSignedDoc(viewModelParam.FileId, (int)viewModel.FileId);
                    }
                    return true;
                }

            }
            return false;
        }

        public async Task<bool> ProcessSignedDocumentsAndSaveToSharePoint(DocDocumentListViewModel viewModelParam, string accessToken, bool needGraphClient = false)
        {
            var envelopeParam = new DocuSignEnvelopeGetParam()
            {
                AuthServer = _docuSignSettings.AuthServer,
                AccessToken = accessToken,
                EnvelopeId = viewModelParam.EnvelopeId
            };

            var envelope = await GetEnvelopeData(envelopeParam);
            if (envelope.Status.ToLower() == "completed")
            {
                var stream = GetSignedDocuments(envelopeParam);
                if (stream != null)
                {
                    var recKey = "";
                    switch (viewModelParam.DocLibraryFolder)
                    {
                        case SharePointDocLibraryFolder.Invention:
                            var inv = await _repository.Inventions.Where(r => r.InvId == viewModelParam.ParentId).FirstOrDefaultAsync();
                            if (inv != null)
                            {
                                recKey = inv.CaseNumber;
                            }
                            break;
                        case SharePointDocLibraryFolder.Application:
                            var ca = await _repository.CountryApplications.Where(r => r.AppId == viewModelParam.ParentId).FirstOrDefaultAsync();
                            if (ca != null)
                            {
                                recKey = SharePointViewModelService.BuildRecKey(ca.CaseNumber, ca.Country, ca.SubCase);
                            }
                            break;
                        case SharePointDocLibraryFolder.Trademark:
                            var tmk = await _repository.TmkTrademarks.Where(r => r.TmkId == viewModelParam.ParentId).FirstOrDefaultAsync();
                            if (tmk != null)
                            {
                                recKey = SharePointViewModelService.BuildRecKey(tmk.CaseNumber, tmk.Country, tmk.SubCase);
                            }
                            break;

                            //case SharePointDocLibraryFolder.Action:
                            //    break;
                            //case SharePointDocLibraryFolder.Cost:
                            //    break;
                    }

                    if (!string.IsNullOrEmpty(recKey))
                    {
                        var folders = SharePointViewModelService.GetDocumentFolders(viewModelParam.DocLibraryFolder, recKey);
                        var graphClient = _sharePointService.GetGraphClient();

                        if (needGraphClient == true)
                            graphClient = _sharePointService.GetGraphClientByClientCredentials();

                        var docName = viewModelParam.DocName.Split(".");
                        var fileName = $"{docName[0]}-Signed.pdf";
                        var result = await graphClient.UploadSiteFile(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, viewModelParam.DocLibrary, folders, stream, fileName);
                        if (!string.IsNullOrEmpty(result.DriveItemId))
                        {
                            await _docService.MarkSignedDocForSharePoint(viewModelParam.Id, result.DriveItemId, fileName);
                        }
                    }
                    return true;
                }
            }
            return false;
        }

        public async Task<bool> ProcessSignedDocsOutAndSaveToSharePoint(object viewModelParam, string accessToken, bool needGraphClient = false)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> ProcessSignedDocsOutAndSave(object viewModelParam, string accessToken)
        {
            throw new NotImplementedException();
        }

        private async System.Threading.Tasks.Task SendToListener(string envelopeId)
        {
            try
            {
                var settings = await _settings.GetSetting();

                if (!string.IsNullOrEmpty(settings.DocuSignWebhookUrl))
                {
                    HttpClient httpClient = new HttpClient();
                    var url = settings.DocuSignWebhookUrl.TrimEnd('/') + "/api/cpidocusignstatus";
                    var data = new DocuSignListenerDTO()
                    {
                        AccountId = _docuSignSettings.ClientId,
                        ImpersonatedUserId = _docuSignSettings.ImpersonatedUserId,
                        ClientUrl = _httpContextAccessor.HttpContext.Request.Scheme + "://" + _httpContextAccessor.HttpContext.Request.Host + _httpContextAccessor.HttpContext.Request.PathBase,
                        EnvelopeIds = new List<string>() { envelopeId }
                    };
                    var json = JsonConvert.SerializeObject(data);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await httpClient.PostAsync(url, content);

                    if (response.IsSuccessStatusCode)
                    {
                        //var result = await response.Content.ReadAsStringAsync();
                        //return result;
                    }
                    else
                    {
                        //throw new Exception($"Failed to send POST request: {response.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                var error = ex.Message;
            }
        }

        public async Task<List<DocFileSignatureRecipient>> GetDocuSignRecipients(string envelopeId)
        {
            return await _docService.DocFileSignatureRecipients.AsNoTracking().Where(d => d.EnvelopeId == envelopeId).ToListAsync();            
        }
    }
}
