using Kendo.Mvc.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OpenIddict.Validation.AspNetCore;
using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Entities.Shared;
using R10.Core.Helpers;
using R10.Core.Interfaces;
// using R10.Core.Interfaces.DMS; // Removed during deep clean
using R10.Core.Interfaces.Shared;
using R10.Web.Areas.Shared.Controllers;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;
using R10.Web.Filters;
using R10.Web.Helpers;
using R10.Web.Interfaces;
using R10.Web.Models;
using R10.Web.Security;
using R10.Web.Services;
using R10.Web.Services.DocumentStorage;
using R10.Web.Services.SharePoint;
using System.Globalization;
using System.Net.Mail;
using System.Reflection;
using System.Text.RegularExpressions;

namespace R10.Web.Api.Shared
{
    [Route("api/docusign")]    
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme, Policy = SharedAuthorizationPolicy.CanAccessSystem)]
    [ApiController]
    public class DocuSignController : ControllerBase
    {        
        private readonly ISystemSettings<DefaultSetting> _defaultSettings;
        private readonly IDocuSignService _docuSignService;       
        private readonly IDocumentService _docService;

//         private readonly IDisclosureService _disclosureService; // Removed during deep clean

        private readonly IOuickEmailViewModelService _quickEmailViewModelService;        
        private readonly ICPiUserGroupManager _userGroupManager;        
        private readonly IStringLocalizer<QuickEmailResource> _qeLocalizer;
        private readonly IEmailSender _emailSender;
        private readonly INotificationSettingManager _userSettingManager;
        private readonly ISharePointService _sharePointService;
        private readonly ISharePointViewModelService _sharePointViewModelService;
        private readonly GraphSettings _graphSettings;
        private readonly IQuickEmailService _qeService;
        private readonly IDocumentHelper _documentHelper;
        private readonly IDocsOutService _docsOutService;

        public DocuSignController(         
            ISystemSettings<DefaultSetting> defaultSettings,  
            IDocuSignService docuSignService,
            IDocumentService docService,
//             IDisclosureService disclosureService, // Removed during deep clean
            IOuickEmailViewModelService quickEmailViewModelService,
            ICPiUserGroupManager userGroupManager,
            IStringLocalizer<QuickEmailResource> qeLocalizer,
            IEmailSender emailSender,
            INotificationSettingManager userSettingManager,
            ISharePointViewModelService sharePointViewModelService,
            ISharePointService sharePointService, 
            IOptions<GraphSettings> graphSettings,
            IQuickEmailService qeService,
            IDocumentHelper documentHelper,
            IDocsOutService docsOutService
            )
        {                     
            _defaultSettings = defaultSettings;  
            _docuSignService = docuSignService;
            _docService = docService;
            _quickEmailViewModelService = quickEmailViewModelService;
            _userGroupManager = userGroupManager;
            _qeLocalizer = qeLocalizer;
            _emailSender = emailSender;
            _userSettingManager = userSettingManager;

            _qeService = qeService;
            _documentHelper = documentHelper;
            _sharePointService = sharePointService;
            _sharePointViewModelService = sharePointViewModelService;
            _graphSettings = graphSettings.Value;

            _docsOutService = docsOutService;
        }

        [HttpPost("updatedocusignstatus")]
        public async Task<IActionResult> UpdateDocuSignStatus([FromBody] DocuSignEnvelopeUpdateDTO updateDTO)
        {
            if (updateDTO != null)
            {
                var settings = await _defaultSettings.GetSetting();

                //Update envelope status
                if (settings.DocumentStorage == DocumentStorageOptions.SharePoint)
                    await _docService.UpdateSharePointSignatureStatus(updateDTO.EnvelopeId, updateDTO.Status);
                else
                    await _docService.UpdateFileSignatureStatus(updateDTO.EnvelopeId, updateDTO.Status);

                await _docService.UpdateLetSignatureStatus(updateDTO.EnvelopeId, updateDTO.Status);
                await _docService.UpdateEFSSignatureStatus(updateDTO.EnvelopeId, updateDTO.Status);

                //Update recipient status
                if (updateDTO.Recipients != null && updateDTO.Recipients.Count > 0)
                    await _docService.UpdateSignatureRecipientStatus(updateDTO.EnvelopeId, updateDTO.Recipients);

                //Pull signed document if status is completed
                DocuSignEnvelopeStatus envelopeStatus;
                if (Enum.TryParse(updateDTO.Status, out envelopeStatus))
                {
                    if (Enum.IsDefined(typeof(DocuSignEnvelopeStatus), envelopeStatus))
                    {
                        if (envelopeStatus == DocuSignEnvelopeStatus.completed)
                        {
                            var accessToken = _docuSignService.GetDocuSignAccessToken(); 
                            if (accessToken.ContainsKey("AccessToken"))
                            {
                                var accessTokenValue = accessToken.GetValueOrDefault("AccessToken");
                                if (!string.IsNullOrEmpty(accessTokenValue))
                                    await ProcessCompletedDocumentUpdate(settings, updateDTO, accessTokenValue);
                            }
                        }                        
                    }
                }
                return Ok();
            }
            return BadRequest();
        }

        #region Helpers
        private async Task ProcessCompletedDocumentUpdate(DefaultSetting settings, DocuSignEnvelopeUpdateDTO updateDTO, string accessTokenValue)
        {            
            var accessToken = accessTokenValue;

            if (settings.DocumentStorage == DocumentStorageOptions.SharePoint)
            {
                await ProcessSharePointDocuments(updateDTO.EnvelopeId, accessToken);
            }
            else
            {
                await ProcessBlobDocuments(updateDTO.EnvelopeId, accessToken);
            }
            
            await ProcessLetterLogs(updateDTO.EnvelopeId, accessToken, settings.DocumentStorage);
            await ProcessEFSLogs(updateDTO.EnvelopeId, accessToken, settings.DocumentStorage);
        }

        private async Task ProcessLetterLogs(string envelopeId, string accessToken, DocumentStorageOptions storageOption)
        {            
            var letSignature = await _docService.LetterLogs
                .Where(d => d.EnvelopeId == envelopeId && d.SignedLetLogId == null)
                .Select(d => new
                {
                    d.LetLogId,
                    d.EnvelopeId,
                    d.ScreenId,
                    d.SystemType,
                    ParentId = d.LetterLogDetails.First().DataKeyValue,
                    d.SignatureCompleted,
                    d.LetFile
                })
                .FirstOrDefaultAsync();

            if (letSignature == null || letSignature.SignatureCompleted == true) return;
                        
            var hasSignedFile = await _docService.LetterLogs.AnyAsync(d => d.EnvelopeId == envelopeId && d.SignedLetLogId != null);
            if (hasSignedFile) return;

            var screenCode = "";
            var systemScreen = await _docsOutService.GetScreenInfo(letSignature.ScreenId);
            if (systemScreen != null && !string.IsNullOrEmpty(systemScreen.ScreenCode))
            {
                screenCode = systemScreen.ScreenCode.Split("-")[0];
            }

            var viewModel = new DocsOutSignatureSignedViewModel
            {
                DocLogId = letSignature.LetLogId,
                ParentId = letSignature.ParentId,
                EnvelopeId = letSignature.EnvelopeId,
                LetFile = letSignature.LetFile,
                ScreenCode = screenCode,
                SystemTypeCode = letSignature.SystemType,
                DocumentCode = "Let"
            };

            if (storageOption == DocumentStorageOptions.SharePoint)
            {
                await _docuSignService.ProcessSignedDocsOutAndSaveToSharePoint(viewModel, accessToken, true);
            }
            else
            {
                await _docuSignService.ProcessSignedDocsOutAndSave(viewModel, accessToken);
            }
        }

        private async Task ProcessEFSLogs(string envelopeId, string accessToken, DocumentStorageOptions storageOption)
        {
            var efsSignature = await _docService.EFSLogs
                .FirstOrDefaultAsync(d => d.EnvelopeId == envelopeId && d.SignedEfsLogId == null && d.SignatureCompleted != true);

            if (efsSignature == null) return;

            var hasSignedFile = await _docService.EFSLogs.AnyAsync(d => d.EnvelopeId == envelopeId && d.SignedEfsLogId != null);
            if (hasSignedFile) return;

            var screenCode = efsSignature.DataKey.ToLower() switch
            {
                "appid" => ScreenCode.Application,
                "invid" => ScreenCode.Invention,
                "tmkid" => ScreenCode.Trademark,
                _ => ""
            };

            var viewModel = new DocsOutSignatureSignedViewModel
            {
                DocLogId = efsSignature.EfsLogId,
                ParentId = efsSignature.DataKeyValue ?? 0,
                EnvelopeId = efsSignature.EnvelopeId,
                LetFile = efsSignature.EfsFile,
                ScreenCode = screenCode,
                SystemTypeCode = efsSignature.SystemType,
                DocumentCode = "EFS"
            };

            if (storageOption == DocumentStorageOptions.SharePoint)
            {
                await _docuSignService.ProcessSignedDocsOutAndSaveToSharePoint(viewModel, accessToken, true);
            }
            else
            {
                await _docuSignService.ProcessSignedDocsOutAndSave(viewModel, accessToken);
            }
        }

        #region Blob
        private async Task ProcessBlobDocuments(string envelopeId, string accessToken)
        {
            var signature = await _docService.DocFileSignatures
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.EnvelopeId == envelopeId && d.SignatureCompleted != true);

            if (signature == null)
            {
                return; // Early exit if no valid signature is found
            }

            var docName = await _docService.DocDocuments
                .AsNoTracking()
                .Where(d => d.FileId == signature.FileId)
                .Select(d => d.DocName)
                .FirstOrDefaultAsync();

            var docVM = new DocDocumentListViewModel
            {
                EnvelopeId = envelopeId,
                ParentId = signature.DataKeyValue,
                SystemType = signature.SystemType,
                ScreenCode = signature.ScreenCode,
                DataKey = signature.DataKey,
                FileId = signature.FileId,
                DocName = docName
            };
            await _docuSignService.ProcessSignedDocumentsAndSave(docVM, accessToken);

            await SendLocalEmailWorkflows(envelopeId);
        }

        private async Task SendLocalEmailWorkflows(string envelopeId)
        {
            var workflows = await GetWorkflowEmails(envelopeId);
            await SendEmailWorkflows(workflows);
        }

        private async Task<List<QuickEmailScreenParameterViewModel>> GetWorkflowEmails(string envelopeId)
        {
            var signatureFile = await _docService.DocFileSignatures.AsNoTracking().Where(d => d.EnvelopeId == envelopeId).FirstOrDefaultAsync();
            var emailWorkflows = new List<QuickEmailScreenParameterViewModel>();
            if (signatureFile != null)
            {
                if (signatureFile.ScreenCode == ScreenCode.DMS && signatureFile.DataKey.ToLower() == "dmsid")
                {
                    DocuSignEnvelopeStatus envelopeStatus;
                    if (Enum.TryParse(signatureFile.EnvelopeStatus, out envelopeStatus))
                    {
                        if (Enum.IsDefined(typeof(DocuSignEnvelopeStatus), envelopeStatus))
                        {
                            //DMS Ready for Submission
                            if (envelopeStatus == DocuSignEnvelopeStatus.completed)
                            {
//                                 // DMS disclosure workflow removed (IDisclosureViewModelService no longer used) // Removed during deep clean
                            }
                        }
                    }
                }
            }
            return emailWorkflows;
        }
        #endregion

        #region SharePoint
        private async Task ProcessSharePointDocuments(string envelopeId, string accessToken)
        {
            var signature = await _docService.SharePointFileSignatures
                .FirstOrDefaultAsync(d => d.EnvelopeId == envelopeId && d.SignatureCompleted != true);

            if (signature == null) return;

            var docVM = new DocDocumentListViewModel
            {
                EnvelopeId = envelopeId,
                ParentId = signature.ParentId,
                DocLibraryFolder = signature.DocLibraryFolder,
                DocLibrary = signature.DocLibrary,
                DocName = signature.FileName,
                Id = signature.DriveItemId
            };
            await _docuSignService.ProcessSignedDocumentsAndSaveToSharePoint(docVM, accessToken, true);

            await SendSharePointEmailWorkflows(envelopeId);
        }

        private async Task SendSharePointEmailWorkflows(string envelopeId)
        {
            var workflows = await GetSharePointWorkflowEmails(envelopeId);
            await SendEmailWorkflows(workflows);
        }

        private async Task<List<QuickEmailScreenParameterViewModel>> GetSharePointWorkflowEmails(string envelopeId)
        {
            var signatureFile = await _docService.SharePointFileSignatures.AsNoTracking().Where(d => d.EnvelopeId == envelopeId).FirstOrDefaultAsync();
            var emailWorkflows = new List<QuickEmailScreenParameterViewModel>();
            if (signatureFile != null)
            {
                if (signatureFile.DocLibraryFolder == SharePointDocLibraryFolder.DMS)
                {
                    DocuSignEnvelopeStatus envelopeStatus;
                    if (Enum.TryParse(signatureFile.EnvelopeStatus, out envelopeStatus))
                    {
                        if (Enum.IsDefined(typeof(DocuSignEnvelopeStatus), envelopeStatus))
                        {
                            //DMS Ready for Submission
                            if (envelopeStatus == DocuSignEnvelopeStatus.completed)
                            {
//                                 // DMS disclosure workflow removed (IDisclosureViewModelService no longer used) // Removed during deep clean
                            }
                        }
                    }
                }
            }
            return emailWorkflows;
        }
        #endregion

        private async Task SendEmailWorkflows(List<QuickEmailScreenParameterViewModel> workflows)
        {
            if (workflows.Count == 0) return;

            foreach (var wf in workflows.Where(d => d.SendImmediately))
            {
                await SendEmailWorkflow(wf.QESetupId ?? 0, wf.ParentKey, wf.ParentId, wf.ParentTable, wf.SystemType, wf.RoleLink, wf.SharePointDocLibrary, wf.SharePointDocLibraryFolder, wf.SharePointRecKey, wf.IncludeImages);
            }
        }
                
        #endregion

        #region Email Helpers
        private async Task SendEmailWorkflow(int qeSetupId, string parentKey, int parentId, string parentTable, string systemType, string roleLink, string? docLibrary = "", string? docLibraryFolder = "", string? recKey = "", bool includeImages = false)
        {
            var quickEmail = await _quickEmailViewModelService.GetQuickEmailById(qeSetupId);

            if (quickEmail == null)
                return;

            quickEmail.LanguageCulture = "en";
            quickEmail.SystemType = systemType;
            quickEmail.ParentKey = parentKey;
            quickEmail.ParentId = parentId;
            quickEmail.ParentTable = parentTable;
            quickEmail.ScreenName = quickEmail.ScreenName;
            quickEmail.RoleLink = roleLink;
            quickEmail.ParentScreenName = QuickEmailHelper.GetParentScreenName(systemType, quickEmail.ScreenName);
            quickEmail.IncludeImages = includeImages;
            quickEmail.DisplayImages = false;
            quickEmail.To = await GenerateEmailAddresses(quickEmail, "To");
            quickEmail.CopyTo = await GenerateEmailAddresses(quickEmail, "Copy to");
            quickEmail.FromAddress = "no-reply@computerpackages.com";
            //quickEmail.ReplyToAddress = GetReplyToAddress(quickEmail.ReplyToUseSender, quickEmail.ReplyToAddress);
            var parentData = await GetParentData(quickEmail.ParentScreenName, new ParentDataStrategyParam { Id = quickEmail.ParentId, SharePointDocLibrary = docLibrary, SharePointDocLibraryFolder = docLibraryFolder, SharePointRecKey = recKey });
            var customFieldData = await _quickEmailViewModelService.GetCustomFieldData(quickEmail.DataSourceID, quickEmail.ParentId);
            quickEmail.Subject = GenerateSubject(quickEmail.Subject, parentData, quickEmail.LanguageCulture);

            if (customFieldData != null && customFieldData.Count > 0)
                quickEmail.Body = GenerateBody(quickEmail, parentData, customFieldData);
            else
                quickEmail.Body = GenerateBody(quickEmail, parentData);

            //quickEmail.Bcc = string.Empty;

            var doNotEmailList = await _userSettingManager.GetDoNotSendQuickEmailList(quickEmail.OptOutSetting, Convert.ToChar(quickEmail.SystemType ?? ""));

            if (string.IsNullOrEmpty(quickEmail.To))
                return;
            //quickEmail.To = User.Identity.Name;

            if (!string.IsNullOrEmpty(quickEmail.FromAddress))
                _emailSender.From = new MailAddress(quickEmail.FromAddress);

            _emailSender.To = GetAddresses(quickEmail.To, doNotEmailList);
            _emailSender.Cc = GetAddresses(quickEmail.CopyTo, doNotEmailList);
            _emailSender.Bcc = GetAddresses(quickEmail.Bcc, doNotEmailList);
            _emailSender.ReplyTo = GetAddresses(quickEmail.ReplyToAddress, doNotEmailList);
            //await AttachImages(quickEmail.SystemType, quickEmail.Images);
            //await AttachFiles(quickEmail.Files);
            //await AttachPreviousAttachments(quickEmail); //from QE log

            if (_emailSender.To.Any())
            {
                var result = await _emailSender.SendEmailAsync(quickEmail.Subject, quickEmail.Body);

                if (result.Success)
                    await LogEmail(quickEmail);

                //// delete selected images and uploaded files in users Temporary folder
                //DeleteAttachedFiles();             
            }
        }

        private async Task<object> GetParentData(ScreenName screenName, ParentDataStrategyParam param)
        {
            var context = new ParentDataContext(_quickEmailViewModelService, screenName, User);
            var data = await context.GetData(screenName, param);
            return data;
        }

        private string GenerateSubject(string subject, object data, string languageCulture)
        {
            if (string.IsNullOrEmpty(subject) || data == null)
                return subject;

            return ReplaceMergeFields(subject, data, languageCulture);
        }

        private string GenerateBody(QuickEmailDetailViewModel quickEmail, object data)
        {
            var body = BuildBody(quickEmail);

            if (string.IsNullOrEmpty(quickEmail.Detail) || data == null)
                return body;

            return ReplaceMergeFields(body, data, quickEmail.LanguageCulture);
        }

        private string GenerateBody(QuickEmailDetailViewModel quickEmail, object data, List<QEFieldListDTO> customFieldData)
        {
            var body = BuildBody(quickEmail);

            if (string.IsNullOrEmpty(quickEmail.Detail) || data == null)
                return body;

            var result = ReplaceMergeFields(body, data, quickEmail.LanguageCulture);
            result = ReplaceMerageFieldsWithCustomFields(result, customFieldData, quickEmail.LanguageCulture);

            return result;
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
                                    if (newValue.StartsWith("<table") && !languageCulture.StartsWith("en"))
                                    {
                                        newValue = LocalizeChildTable(newValue, languageCulture);
                                    }
                                }

                            }
                        }
                        else
                            newValue = ((DateTime?)property.GetValue(data)).FormatToDisplay(languageCulture);
                    }

                    text = text.Replace(oldValue, newValue);
                }
            }

            //Domain Url
            var domainUrl = "{{DomainURL}}";
            if (text.Contains(domainUrl))
            {
                text = text.Replace(domainUrl, Request.Scheme + "://" + Request.Host + Request.PathBase);
            }

            return text;
        }

        private string ReplaceMerageFieldsWithCustomFields(string text, List<QEFieldListDTO> customFieldData, string languageCulture)
        {
            foreach (QEFieldListDTO data in customFieldData)
            {
                var oldValue = "{{" + data.ColumnName + "}}";
                if (text.Contains(oldValue))
                {
                    var newValue = "";
                    if (data.DataType != null)
                    {
                        //var isDate = data.DataType.ToLower().Contains("date");//property.PropertyType == typeof(DateTime?) || property.PropertyType == typeof(DateTime);
                        //TO DO other data type
                        //if (!isDate)
                        //{
                        //    var isDouble = property.PropertyType == typeof(double);
                        //    if (isDouble)
                        //        newValue = ((double)property.GetValue(data)).FormatToDisplay(languageCulture);
                        //    else
                        //    {
                        //        var isDecimal = property.PropertyType == typeof(decimal);
                        //        if (isDecimal)
                        //            newValue = ((decimal)property.GetValue(data)).FormatToDisplay(languageCulture);
                        //        else
                        //        {
                        //            newValue = property.GetValue(data).ToString();
                        //            if (newValue.StartsWith("<table") && !languageCulture.StartsWith("en"))
                        //            {
                        //                newValue = LocalizeChildTable(newValue, languageCulture);
                        //            }
                        //        }

                        //    }
                        //}
                        //else
                        newValue = string.IsNullOrEmpty(data.DataValue) ? "" : DateTime.Parse(data.DataValue).ToString("dd-MMM-yyyy", CultureInfo.CreateSpecificCulture(languageCulture));
                    }

                    text = text.Replace(oldValue, newValue);
                }
            }
            return text;
        }

        private string BuildBody(QuickEmailDetailViewModel viewModel)
        {
            var body = string.Empty; //viewModel.Header + "<br/>" + viewModel.Detail + "<br/>" + viewModel.Footer;

            if (viewModel.Header != "")
                body = viewModel.Header + "<br/>";

            if (viewModel.Detail != "")
                body = body + viewModel.Detail + "<br/>";

            if (viewModel.Footer != "")
                body = body + viewModel.Footer;

            return body.Replace("&lt;", "<").Replace("&gt;", ">").Replace("&amp;", "&");
        }

        private string LocalizeChildTable(string data, string languageCulture)
        {
            string body = Regex.Match(data, "<tbody>(.*?)</tbody>").Groups[1].ToString();
            if (!string.IsNullOrEmpty(body))
            {
                var cells = Regex.Matches(body, "<td data-type.*?>(.*?)</td>");
                for (var i = 0; i < cells.Count; i++)
                {
                    var origValue = cells[i].Groups[1].ToString();
                    if (!string.IsNullOrEmpty(origValue))
                    {
                        string newValue;
                        if (cells[i].Groups[0].ToString().Contains("qe-date"))
                        {
                            var date = (DateTime?)Convert.ToDateTime(origValue);
                            newValue = date.FormatToDisplay(languageCulture);
                        }
                        else
                        {
                            var number = (Double)Convert.ToDouble(origValue);
                            newValue = number.FormatToDisplay(languageCulture);
                        }
                        var td = cells[i].Groups[0].ToString().Replace(origValue, newValue);
                        data = data.Replace(cells[i].Groups[0].ToString(), td);
                    }
                }
            }

            //below must follow after above, date and number data conversion must be done first
            //using the default db server format
            var currentCulture = CultureInfo.CurrentCulture;
            CultureInfo.CurrentCulture = new CultureInfo(languageCulture);
            CultureInfo.CurrentUICulture = new CultureInfo(languageCulture);

            string head = Regex.Match(data, "<thead>(.*?)</thead>").Groups[1].ToString();
            if (!string.IsNullOrEmpty(head))
            {
                var cells = Regex.Matches(head, "<td.*?>(.*?)</td>");
                for (var i = 0; i < cells.Count; i++)
                {
                    var label = cells[i].Groups[1].ToString();
                    var localizedLabel = _qeLocalizer[label].ToString();
                    var td = cells[i].Groups[0].ToString().Replace(label, localizedLabel);
                    data = data.Replace(cells[i].Groups[0].ToString(), td);
                }
            }

            CultureInfo.CurrentCulture = currentCulture;
            CultureInfo.CurrentUICulture = currentCulture;
            return data;
        }

        private List<MailAddress> GetAddresses(string? addresses, List<string> doNotEmailList)
        {
            var newAddresses = new List<MailAddress>();

            if (addresses == null)
                return newAddresses;

            addresses = addresses.Replace(",", ";");
            foreach (var address in addresses.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries).Select(e => e.Trim()).Distinct())
            {
                if (!string.IsNullOrEmpty(address) && !doNotEmailList.Any(e => e.IsCaseInsensitiveEqual(address.Trim())))
                    newAddresses.Add(new MailAddress(address));
            }
            return newAddresses;
        }

        private async Task<string> GenerateEmailAddresses(QuickEmailDetailViewModel quickEmail, string sendAs)
        {
            var recipients = await _quickEmailViewModelService.GetDefaultRecipients(quickEmail.QESetupID, sendAs);
            if (recipients == null)
                return string.Empty;

            var emails = string.Empty;
            var lastChar = string.Empty;
            const string separator = ";";

            var customSources = new List<string> { "tblpuboptions", "custom", "usergroup" };
            //var mainRecipients = recipients.Where(r => r.QERoleSource.SourceSQL != "tblPubOptions" && r.QERoleSource.SourceSQL.ToLower() != "custom").ToList();
            var mainRecipients = recipients.Where(r => !customSources.Contains(r.QERoleSource.SourceSQL.ToLower())).ToList();
            foreach (var recipient in mainRecipients)
            {
                var email = await _quickEmailViewModelService.GetRecipientEmail(quickEmail.RoleLink, recipient.QERoleSource);
                if (!string.IsNullOrEmpty(email))
                {
                    if (!string.IsNullOrEmpty(emails))
                        lastChar = emails.Substring(emails.Length - 1);

                    if (lastChar == separator)
                        emails = emails + " " + email + ";";
                    else
                        emails = emails + "; " + email + ";";
                }
            }
            var otherRecipients = recipients.Where(r => r.QERoleSource.SourceSQL == "tblPubOptions").FirstOrDefault();
            if (otherRecipients != null)
            {
                var email = (await _defaultSettings.GetSetting()).FirmContact;
                if (!string.IsNullOrEmpty(emails))
                    lastChar = emails.Substring(emails.Length - 1);

                if (lastChar == separator)
                    emails = emails + " " + email + ";";
                else
                    emails = emails + "; " + email + ";";
            }
            var customRecipients = recipients.Where(r => r.QERoleSource.SourceSQL.ToLower() == "custom").ToList();
            foreach (var recipient in customRecipients)
            {
                var email = recipient.QERoleSource.RoleName;
                if (!string.IsNullOrEmpty(email))
                {
                    if (!string.IsNullOrEmpty(emails))
                        lastChar = emails.Substring(emails.Length - 1);

                    if (lastChar == separator)
                        emails = emails + " " + email + ";";
                    else
                        emails = emails + "; " + email + ";";
                }
            }
            var userGroupRecipients = recipients.Where(r => r.QERoleSource.SourceSQL.ToLower() == "usergroup").ToList();
            foreach (var recipient in userGroupRecipients)
            {
                //Get user detail from group
                var userEmails = await _userGroupManager.CPiUserGroups.Where(d => d.CPiGroup.Name.ToLower() == recipient.QERoleSource.RoleName.ToLower() && d.CPiGroup.IsEnabled)
                                        .Select(d => d.CPiUser.Email).ToListAsync();
                foreach (var email in userEmails)
                {
                    if (!string.IsNullOrEmpty(email))
                    {
                        if (!string.IsNullOrEmpty(emails))
                            lastChar = emails.Substring(emails.Length - 1);

                        if (lastChar == separator)
                            emails = emails + " " + email + ";";
                        else
                            emails = emails + "; " + email + ";";
                    }
                }
            }

            if (string.IsNullOrEmpty(emails))
                return emails;
            else
                return emails.Substring(2);
        }

        public async Task<int> LogEmail(QuickEmailDetailViewModel viewModel)
        {
            var genDate = DateTime.Now;
            var systemTypeName = QuickEmailHelper.GetSystem(viewModel.SystemType);
            var qeLog = new QELog
            {
                SystemType = viewModel.SystemType,
                SystemTypeName = systemTypeName,
                QESetupId = viewModel.QESetupID,
                TemplateName = await GetTemplateName(viewModel.QESetupID),
                ScreenId = viewModel.ScreenId,
                DataKey = viewModel.LogParentKey != null ? viewModel.LogParentKey : viewModel.ParentKey,
                DataKeyValue = viewModel.LogParentId != null && viewModel.LogParentId > 0 ? (int)viewModel.LogParentId : viewModel.ParentId,
                To = viewModel.To,
                Cc = viewModel.CopyTo,
                From = viewModel.FromAddress,
                ReplyTo = viewModel.ReplyToAddress,
                Bcc = viewModel.Bcc,
                Subject = viewModel.Subject,
                Body = viewModel.Body,
                GenBy = User.GetUserName(),
                GenDate = genDate,
                RoleLink = viewModel.RoleLink,
                DataSourceID = viewModel.DataSourceID,
                ImageParentId = viewModel.ImageParentId,
                ScreenCode = viewModel.ScreenCode,
                SharePointDocLibrary = viewModel.SharePointDocLibrary,
                SharePointDocLibraryFolder = viewModel.SharePointDocLibraryFolder,
                SharePointRecKey = viewModel.SharePointRecKey
            };

            if (!string.IsNullOrEmpty(viewModel.Attachments) && viewModel.Attachments != "[]")
                qeLog.Attachments = viewModel.Attachments; //from QE log
            else
                qeLog.Attachments = await GetAttachments(viewModel, qeLog); //new email

            var message = new MsgEmailModel
            {
                From = _emailSender.From,
                To = _emailSender.To,
                Cc = _emailSender.Cc,
                Bcc = _emailSender.Bcc,
                ReplyTo = _emailSender.ReplyTo,
                Subject = viewModel.Subject,
                Body = viewModel.Body,
                SentDate = genDate,
                IsHtml = true,
                Attachments = _emailSender.Attachments
            };

            var settings = await _defaultSettings.GetSetting();

            if (settings.IsSharePointIntegrationOn && settings.IsSharePointLoggingOn)
            {
                var email = _documentHelper.CreateMsgFile(message);
                var fileName = $"{DateTime.Now:yyyy-MM-dd-hhmmsstt}-{User.GetUserIdentifier()}.msg";
                using (var stream = new MemoryStream())
                {
                    email.Save(stream);

                    var sharePointSystemFolder = _sharePointViewModelService.GetDocLibraryFromSystemTypeCode(viewModel.SystemType);
                    var folders = new List<string> { sharePointSystemFolder };
                    stream.Position = 0;

                    //var graphClient = _sharePointService.GetGraphClient();
                    var graphClient = _sharePointService.GetGraphClientByClientCredentials();
                    var result = await graphClient.UploadSiteFile(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, SharePointDocLibrary.QELog, folders, stream, fileName);
                    qeLog.ItemId = result.DriveItemId;
                }
            }
            else
            {
                var documentHeader = new DocumentStorageHeader
                {
                    SystemType = viewModel.SystemType.ToUpper(),                                // global search consistency
                    ScreenCode = _documentHelper.DataKeyToScreenCode(qeLog.DataKey),            // global search consistency
                    DocumentType = DocumentLogType.EmailLog,
                    //DataKey= qeLog.DataKey,                                                   // replaced with ScreenCode
                    ParentId = qeLog.DataKeyValue.ToString()
                };
                qeLog.QEFile = _documentHelper.SaveEmailToMsgFile(message, systemTypeName, documentHeader);

            }

            await _qeService.AddLogAsync(qeLog);
            return qeLog.LogID;
        }

        protected async Task<string> GetTemplateName(int id)
        {
            var quickEmail = await _quickEmailViewModelService.GetQuickEmailById(id);
            return quickEmail == null ? string.Empty : quickEmail.TemplateName;
        }

        protected async Task<string> GetAttachments(QuickEmailDetailViewModel viewModel, QELog qeLog)
        {
            //var attachments = await LogUploadedFiles(viewModel.Files, qeLog);
            var selectedImages = await LogSelectedImages(qeLog, viewModel.Images);

            //if (selectedImages.Count > 0)
            //    attachments.AddRange(selectedImages);

            return JsonConvert.SerializeObject(selectedImages);
        }

        private async Task<List<AttachedFileDTO>> LogSelectedImages(QELog qeLog, string json)
        {
            var selectedImagesList = new List<AttachedFileDTO>();

            if (string.IsNullOrEmpty(json))
                return selectedImagesList;

            var images = JsonConvert.DeserializeObject<List<AttachedFileDTO>>(json);

            foreach (var image in images)
            {
                var fileId = image.FileId;          // 123
                if (string.IsNullOrEmpty(fileId))
                {
                    fileId = image.FileName;
                }

                var imageTitle = image.FileTitle;   // koala
                var imagePath = image.FileName;     // 123.jpg
                var thumbNail = image.Thumbnail == "null" ? "" : image.Thumbnail;    // 123_thumb.jpg; note: JsonConvert.DeserializeObject returns the string "null" instead of the null value

                var newFileName = "QELog_" + imagePath;
                var newThumbNail = "";

                //await _viewModelService.LogEmailImageAttachment(qeLog, imagePath, newFileName, newThumbNail);

                var attachedFile = new AttachedFileDTO
                {
                    //FileId = Path.GetFileName(imagePath),
                    FileId = fileId,
                    FileName = newFileName,
                    Thumbnail = thumbNail,
                    FileTitle = imageTitle,
                    ItemId = image.ItemId,
                    SharePointDocLibrary = image.SharePointDocLibrary
                };

                selectedImagesList.Add(attachedFile);
            }

            return selectedImagesList;
        }
        #endregion
    }
}
