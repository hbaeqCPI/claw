using ActiveQueryBuilder.Web.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using R10.Core.Entities.Patent;
using R10.Core.Entities;
using R10.Core.Entities.Shared;
using R10.Core.Entities.Trademark;
using R10.Core.Helpers;
using R10.Core.Identity;
using R10.Core.Interfaces;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Filters;
using R10.Web.Helpers;
using R10.Web.Security;
using R10.Web.Services;
using R10.Web.Services.MailDownload;
using Sustainsys.Saml2.Metadata;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
// using R10.Core.Entities.GeneralMatter; // Removed during deep clean
using R10.Web.Interfaces;
using Microsoft.EntityFrameworkCore;
using DocuSign.eSign.Model;
using R10.Web.Extensions;

using R10.Web.Areas;

namespace R10.Web.Areas.Shared.Controllers
{
    public abstract class DocumentUploadController : BaseController
    {
        protected readonly IMailDownloadService _mailDownloadService;
        protected readonly IDocumentsViewModelService _docViewModelService;
        protected readonly GraphSettings _graphSettings;
        protected readonly ILogger<DocDocumentsController> _logger;
        protected readonly ServiceAccount _serviceAccount;
        protected readonly IAuthorizationService _authService;
        protected readonly ISystemSettings<DefaultSetting> _settings;
        protected readonly ISystemSettings<PatSetting> _patSettings;
        protected readonly ISystemSettings<TmkSetting> _tmkSettings;
        // protected readonly ISystemSettings<GMSetting> _gmSettings; // Removed during deep clean
        protected readonly IEPOService _epoService;
        protected readonly IEntityService<EPOCommunication> _epoCommunicationService;

        protected const string Cookie_MailDownloader = "CPI_MailDownloader";

        public DocumentUploadController(
            IMailDownloadService mailDownloadService,
            IDocumentsViewModelService docViewModelService,
            IOptions<GraphSettings> graphSettings,
            IOptions<ServiceAccount> serviceAccount,
            ILogger<DocDocumentsController> logger,
            IAuthorizationService authService,
            ISystemSettings<DefaultSetting> settings,
            ISystemSettings<PatSetting> patSettings, 
            ISystemSettings<TmkSetting> tmkSettings,
            // ISystemSettings<GMSetting> gmSettings, // Removed during deep clean
            IEPOService epoService,
            IEntityService<EPOCommunication> epoCommunicationService)
        {
            _mailDownloadService = mailDownloadService;
            _graphSettings = graphSettings.Value;
            _logger = logger;
            _serviceAccount = serviceAccount.Value;
            _authService = authService;
            _settings = settings;
            _patSettings = patSettings;
            _tmkSettings = tmkSettings;
            // _gmSettings = gmSettings; // Removed during deep clean
            _docViewModelService = docViewModelService;

            _epoService = epoService;
            _epoCommunicationService = epoCommunicationService;
        }

        [ServiceFilter(typeof(MailAuthorizationFilter))]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveDroppedEmails(string[] ids, string[] fileNames, string documentLink, string folderId, string mailbox, string? roleLink)
        {
            //save messages to documents tab
            var droppedFiles = new List<IFormFile>();
            for (int i = 0; i < ids.Length; i++)
            {
                droppedFiles.Add(await _mailDownloadService.DownloadAsFormFile(ids[i], fileNames[i], mailbox));
            }
            var result = await SaveDroppedEmails(droppedFiles, documentLink, folderId, roleLink, null);

            //log downloaded messages first before moving to downloaded items folder
            //moving will create new message ids
            await _mailDownloadService.LogDownloadedMessages(ids, documentLink, mailbox);

            //move messages to downloaded items folder
            var downloadedItemsFolder = await _mailDownloadService.GetDownloadedItemsFolder(mailbox);
            foreach (var id in ids)
            {
                await _mailDownloadService.MoveDownloadedMessage(id, downloadedItemsFolder.Id, mailbox);
            }

            return result;
        }

        [ServiceFilter(typeof(MailAuthorizationFilter))]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveDroppedAttachments(string[] ids, string mailbox, string? roleLink)
        {
            //save attached documents for verification
            var droppedFiles = new List<IFormFile>();
            for (int i = 0; i < ids.Length; i++)
            {
                droppedFiles.AddRange(await _mailDownloadService.DownloadAttachmentsAsFormFiles(ids[i], mailbox));
            }
            var result = await SaveDroppedDocVerification(droppedFiles);

            return result;
        }

        [ServiceFilter(typeof(MailAuthorizationFilter))]
        public async Task<IActionResult> MailDownloadStatus(string mailbox)
        {
            var status = await _mailDownloadService.GetStatus(mailbox);
            if (status.Status == MailDownloadStatusType.Completed)
                return Ok(status);

            return BadRequest(status);
        }

        /// <summary>
        /// Force mail download to start 
        /// by ignoring mail downloader cookie
        /// </summary>
        /// <returns></returns>
        [Authorize(Policy = CPiAuthorizationPolicy.CPiAdmin)]
        public async Task<IActionResult> StartMailDownloader(string mailbox)
        {
            if (string.IsNullOrEmpty(mailbox))
                return BadRequest();

            return await DownloadEmails(mailbox);
        }

        /// <summary>
        /// Stop mail download manually
        /// </summary>
        /// <returns></returns>
        [Authorize(Policy = CPiAuthorizationPolicy.CPiAdmin)]
        public async Task<IActionResult> StopMailDownloader(string mailbox)
        {
            if (string.IsNullOrEmpty(mailbox))
                return BadRequest();

            var status = await _mailDownloadService.GetStatus(mailbox);

            if (status.Status != MailDownloadStatusType.Completed)
                await _mailDownloadService.EndDownload(status.LogId);

            return Ok();
        }

        [ServiceFilter(typeof(MailAuthorizationFilter))]
        public async Task<IActionResult> MailDownloader(string mailbox)
        {
            var mailDownloaderCookie = $"{Cookie_MailDownloader}_{mailbox.Replace(" ", "")}";

            if (string.IsNullOrEmpty(Request.Cookies[mailDownloaderCookie]))
            {
                Response.Cookies.Append(mailDownloaderCookie, DateTime.Now.ToString("o", CultureInfo.InvariantCulture), new CookieOptions()
                {
                    Path = HttpContext.Request.PathBase,
                    Expires = DateTime.Now.AddMinutes(_graphSettings.GetMailSettings(mailbox)?.Download?.IntervalInMinutes ?? 5),
                    HttpOnly = true,
                    Secure = true,
                });

                return await DownloadEmails(mailbox);
            }

            return Ok(new { count = -1 });
        }

        [ServiceFilter(typeof(MailAuthorizationFilter))]
        public async Task<IActionResult> DownloadEmails(string mailbox, int id = 0)
        {
            var count = 0;
            var download = await _mailDownloadService.StartDownload(mailbox, id);

            if (download.LogId > 0)
            {
                var stopProcessing = new List<string>();
                var emailWorkflows = new List<WorkflowEmailViewModel>();
                var eSignatureWorkflows = new List<WorkflowSignatureViewModel>();

                foreach (var job in download.Jobs)
                {
                    var downloadedItemsFolder = await _mailDownloadService.GetDownloadedItemsFolder(job.MailboxName);
                    var downloadFolders = await _mailDownloadService.GetDownloadFolders(job.MailboxName);
                    var downloadFilters = await _mailDownloadService.GetDownloadFilters(job.ActionId);

                    if (downloadFilters.Count > 0)
                    {
                        foreach (var message in job.Messages)
                        {
                            if (stopProcessing.Contains(message.Id))
                                continue;

                            var documentLinks = await _mailDownloadService.GetDocumentLinks(job.ActionType, message, downloadFilters, job.MailboxName);
                            if (documentLinks.Count > 0)
                            {
                                foreach (var documentLink in documentLinks)
                                {
                                    if (job.StopProcessing)
                                        stopProcessing.Add(message.Id);

                                    if (await _mailDownloadService.IsDownloaded(message.InternetMessageId, documentLink))
                                        continue;

                                    var files = new List<IFormFile>() { await _mailDownloadService.DownloadAsFormFile(message.Id, message.GetDownloadFileName(), job.MailboxName) };

                                    if (job.DownloadAttachments && (message.HasAttachments ?? false))
                                        files.AddRange(await _mailDownloadService.DownloadAttachmentsAsFormFiles(message.Id, job.MailboxName));

                                    var result = await SaveDroppedEmails(files, documentLink, "", await _mailDownloadService.GetRoleLink(job.ActionType, documentLink.Split("|")[3]), job.Responsibles);
                                    count++;

                                    await _mailDownloadService.LogDownloadDetail(download.LogId, job.ActionId, job.RuleId, documentLink, message);

                                    GetWorkflows(result, emailWorkflows, eSignatureWorkflows);
                                }

                                if (!job.DoNotMove)
                                {
                                    var downloadFolderId = string.IsNullOrEmpty(job.DownloadFolderId) || !downloadFolders.Any(f => f.Id == job.DownloadFolderId) ? downloadedItemsFolder.Id : job.DownloadFolderId;
                                    try
                                    {
                                        await _mailDownloadService.MoveDownloadedMessage(message.Id, downloadFolderId, job.MailboxName);
                                    }
                                    catch (Exception e)
                                    {
                                        _logger.LogError(e, "Unable to move downloaded message to mail folder {downloadFolderId}", downloadFolderId);
                                    }
                                }
                            }
                        }
                    }
                }

                await _mailDownloadService.EndDownload(download.LogId);

                try
                {
                    await ProcessWorkflows(emailWorkflows, eSignatureWorkflows);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "An error occurred while processing workflows.");
                }
            }

            return Ok(new { count = count });
        }

        private void GetWorkflows(IActionResult result, List<WorkflowEmailViewModel> emailWorkflows, List<WorkflowSignatureViewModel> eSignatureWorkflows)
        {
            var obj = (result as JsonResult)?.Value;
            if (obj == null) return;

            var type = obj.GetType();
            var resultEmailWorkflows = (List<WorkflowEmailViewModel>?)type.GetProperty("emailWorkflows")?.GetValue(obj, null);
            var resultESignatureWorkflows = (List<WorkflowSignatureViewModel>?)type.GetProperty("eSignatureWorkflows")?.GetValue(obj, null);
            
            if (resultEmailWorkflows != null)
                emailWorkflows.AddRange(resultEmailWorkflows);

            if (resultESignatureWorkflows != null)
                eSignatureWorkflows.AddRange(resultESignatureWorkflows);
        }

        private async Task ProcessWorkflows(List<WorkflowEmailViewModel> emailWorkflows, List<WorkflowSignatureViewModel> eSignatureWorkflows)
        {
            if (!emailWorkflows.Any() && !eSignatureWorkflows.Any())
                return;

            using (HttpClient client = new HttpClient(new HttpClientHandler() { AllowAutoRedirect = false }))
            {
                client.BaseAddress = new Uri(Url.ApplicationBaseUrl(Request.Scheme));

                var authToken = await GetAuthToken(client);

                if (string.IsNullOrEmpty(authToken))
                {
                    _logger.LogError("Unable to process workflows.");
                    return;
                }

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

                foreach (var emailWorkflow in emailWorkflows)
                {
                    var url = emailWorkflow.emailUrl;

                    //pageHelper.handleEmailWorkflow
                    //$.get(url, { id: id, sendImmediately: isAutoEmail, qeSetupId: qeSetupId, autoAttachImages: autoAttachImages,
                    //              fileNames: fileNames.join('~'), imageParent: imageParent, emailTo: emailTo, strId: strId  })
                    //sendImmediately = true
                    //  no interactive quick email screen.
                    //autoAttachImages = false
                    //  docs are only attached if doc's IncludeInWorkflow property is true.
                    //  all docs with IncludeInWorkflow are attached not just the doc from mail download.
                    var response = await client.GetAsync($"{url}?id={emailWorkflow.id}&sendImmediately=true&qeSetupId={emailWorkflow.qeSetupId}" +
                        $"&autoAttachImages=false" +
                        //$"&autoAttachImages={emailWorkflow.autoAttachImages}" +
                        $"&fileNames={String.Join("~", emailWorkflow.fileNames ?? new string[] { })}" +
                        $"&imageParent={emailWorkflow.parentId}&emailTo={emailWorkflow.emailTo}&strId={emailWorkflow.strId}");

                    if (response.StatusCode == HttpStatusCode.Redirect)
                    {
                        var redirectResponse = await client.GetAsync(response.Headers.Location);
                        if (!redirectResponse.IsSuccessStatusCode)
                        {
                            var error = await response.GetErrorMessage();
                            _logger.LogError("Email workflow error: HTTP {StatusCode} {Error}", (int)redirectResponse.StatusCode, error);
                        }
                    }
                    else if (!response.IsSuccessStatusCode)
                    {
                        var error = await response.GetErrorMessage();
                        _logger.LogError("Email workflow error: HTTP {StatusCode} {Error}", (int)response.StatusCode, error);
                    }
                }

                if (eSignatureWorkflows.Any())
                {
                    var url = "/Shared/DocuSign/SendEnvelopeFromFileUploadWorkflow";

                    //pageHelper.handleSignatureWorkflow
                    //$.post(url, { workflows: result.eSignatureWorkflows })
                    var body = new Dictionary<string, string> { { "workflows", JsonConvert.SerializeObject(eSignatureWorkflows) } };
                    var encodedContent = new FormUrlEncodedContent(body);
                    var response = await client.PostAsync(url, encodedContent);

                    if (!response.IsSuccessStatusCode)
                    {
                        var error = await response.GetErrorMessage();
                        _logger.LogError("eSignature workflow error: HTTP {StatusCode} {Error}", (int)response.StatusCode, error);
                    }
                }
            }

            return;
        }

        private async Task<string?> GetAuthToken(HttpClient client)
        {
            var url = "/connect/token";
            var body = new Dictionary<string, string>{{ "grant_type", "password" }, { "username", _serviceAccount.UserName }, { "password", _serviceAccount.Password } };
            var encodedContent = new FormUrlEncodedContent(body);
            var response = await client.PostAsync(url, encodedContent);
            var stringResponse = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<Dictionary<string, string>>(stringResponse);

            return result?.GetValueOrDefault("access_token");
        }

        #region Workflow
        protected async Task<WorkflowHeaderViewModel> GenerateWorkflow(string documentLink, List<WorkflowEmailAttachmentViewModel> attachments, bool isNewFileUpload = false, bool hasNewRespDocketing = false, bool hasRespDocketingReassigned = false, bool hasNewRespReporting = false, bool hasRespReportingReassigned = false)
        {
            var documentLinkArray = documentLink.Split("|");
            var systemTypeCode = documentLinkArray[0];
            var screenCode = documentLinkArray[1];
            var parentId = Convert.ToInt32(documentLinkArray[3]);

            switch (screenCode)
            {
                case ScreenCode.Application:
                    return await GenerateCountryAppWorkflow(attachments, isNewFileUpload, hasNewRespDocketing, hasRespDocketingReassigned, hasNewRespReporting, hasRespReportingReassigned);

                case ScreenCode.Trademark:
                    return await GenerateTrademarkWorkflow(attachments, isNewFileUpload, hasNewRespDocketing, hasRespDocketingReassigned, hasNewRespReporting, hasRespReportingReassigned);

                // Removed during deep clean - GeneralMatter
                // case ScreenCode.GeneralMatter:
                //     return await GenerateGMWorkflow(attachments, isNewFileUpload, hasNewRespDocketing, hasRespDocketingReassigned, hasNewRespReporting, hasRespReportingReassigned);

                case ScreenCode.Action:
                    return await GenerateActionWorkflow(systemTypeCode, attachments);
            }
            return new WorkflowHeaderViewModel();
        }

        protected async Task<List<WorkflowSignatureViewModel>> GenerateSignatureWorkflow(WorkflowHeaderViewModel workflowHeader, string documentLink, List<WorkflowEmailAttachmentViewModel> attachments, int parentId, string? roleLink)
        {
            var settings = await _settings.GetSetting();

            if (!settings.IsESignatureOn)
                return new List<WorkflowSignatureViewModel>();

            if (workflowHeader != null && workflowHeader.Workflows != null)
            {
                var eSignatureWorkflows = workflowHeader.Workflows.Where(wf => wf.ActionTypeId == (int)PatWorkflowActionType.eSignature).ToList();
                if (eSignatureWorkflows.Any())
                {
                    var documentLinkArray = documentLink.Split("|");
                    var screenCode = documentLinkArray[1];

                    var wfs = new List<WorkflowSignatureViewModel>();
                    foreach (var wf in eSignatureWorkflows)
                    {
                        if (wf.Attachments != null && wf.Attachments.Any())
                        {
                            foreach (var attachment in wf.Attachments)
                            {
                                var pos = (attachment.OrigFileName ?? "").LastIndexOf(".");
                                var fileName = (attachment.OrigFileName ?? "").Substring(0, pos);
                                var savedFile = (attachment.FileId ?? 0).ToString();
                                var fileNameArray = (attachment.FileName ?? "").Split(".");
                                if (fileNameArray.Any())
                                {
                                    savedFile = savedFile + "." + fileNameArray[1];
                                }
                                var fileId = (attachment.FileId ?? 0);

                                if (!settings.IsESignatureReviewOn)
                                {
                                    wfs.Add(new WorkflowSignatureViewModel
                                    {
                                        QESetupId = wf.ActionValueId,
                                        ParentId = parentId,
                                        UserFile = new WorkflowSignatureDocViewModel { Name = fileName, FileName = savedFile, FileId = fileId },
                                        ScreenCode = screenCode,
                                        SystemTypeCode = documentLinkArray[0],
                                        RoleLink = roleLink
                                    });
                                }

                                await _docViewModelService.MarkForSignature(fileId, documentLink, wf.ActionValueId, (roleLink ?? ""));
                            }
                        }
                    }
                    if (wfs.Any())
                        return wfs;
                }
            }
            return new List<WorkflowSignatureViewModel>();
        }

        protected List<WorkflowEmailViewModel> GenerateEmailWorkflow(WorkflowHeaderViewModel workflowHeader, List<WorkflowEmailAttachmentViewModel> attachments, int parentId)
        {
            if (workflowHeader != null && workflowHeader.Workflows != null)
            {
                //All System workflow settings have SendMail at the top
                var emailWorkflows = workflowHeader.Workflows.Where(wf => wf.ActionTypeId == (int)PatWorkflowActionType.SendEmail).ToList();
                if (emailWorkflows.Any())
                {
                    var wfs = new List<WorkflowEmailViewModel>();
                    foreach (var wf in emailWorkflows)
                    {
                        if (wf.Attachments != null && wf.Attachments.Any())
                        {
                            foreach (var attachment in wf.Attachments)
                            {
                                wfs.Add(new WorkflowEmailViewModel
                                {
                                    isAutoEmail = !wf.Preview,
                                    qeSetupId = wf.ActionValueId,
                                    autoAttachImages = wf.AutoAttachImages,
                                    id = attachment.DocId,
                                    fileNames = new string[] { (attachment.FileName ?? "") }, //actual filename when using blob storage
                                    parentId = parentId,
                                    emailUrl = string.IsNullOrEmpty(wf.EmailUrl) ? workflowHeader.EmailUrl : wf.EmailUrl,
                                    strId = attachment.Id, //SharePoint
                                    emailTo = string.IsNullOrEmpty(wf.EmailTo) ? null : wf.EmailTo,
                                    attachmentFilter=wf.AttachmentFilter
                                });

                            };
                        }
                    }
                    if (wfs.Any())
                        return wfs;
                }
            }
            return new List<WorkflowEmailViewModel>();
        }

        protected async Task<WorkflowHeaderViewModel> GenerateCountryAppWorkflow(List<WorkflowEmailAttachmentViewModel> attachments, bool isNewFileUpload = false, bool hasNewRespDocketing = false, bool hasRespDocketingReassigned = false, bool hasNewRespReporting = false, bool hasRespReportingReassigned = false)
        {
            var settings = await _patSettings.GetSetting();
            if (settings.IsWorkflowOn)
            {
                var emailUrl = Url.Action("Email", "PatImageApp", new { area = "Patent" });
                var parentId = attachments.First().DocParent;

                var newDocRespDocketingUrl = Url.Action("EmailDocRespDocketing", "PatImageApp", new { area = "Patent" });
                var reassignedDocRespDocketingUrl = Url.Action("EmailReassignedDocRespDocketing", "PatImageApp", new { area = "Patent" });

                var newDocRespReportingUrl = Url.Action("EmailDocRespReporting", "PatImageApp", new { area = "Patent" });
                var reassignedDocRespReportingUrl = Url.Action("EmailReassignedDocRespReporting", "PatImageApp", new { area = "Patent" });

                var workflows = await _docViewModelService.GenerateCountryAppWorkflow(attachments, parentId, isNewFileUpload, hasNewRespDocketing, hasRespDocketingReassigned, newDocRespDocketingUrl ?? "", reassignedDocRespDocketingUrl ?? "", false, hasNewRespReporting, hasRespReportingReassigned, newDocRespReportingUrl ?? "", reassignedDocRespReportingUrl ?? "");
                return new WorkflowHeaderViewModel { Id = parentId, EmailUrl = emailUrl, Workflows = workflows };
            }
            return new WorkflowHeaderViewModel();
        }

        protected async Task<WorkflowHeaderViewModel> GenerateTrademarkWorkflow(List<WorkflowEmailAttachmentViewModel> attachments, bool isNewFileUpload = false, bool hasNewRespDocketing = false, bool hasRespDocketingReassigned = false, bool hasNewRespReporting = false, bool hasRespReportingReassigned = false)
        {
            var settings = await _tmkSettings.GetSetting();
            if (settings.IsWorkflowOn)
            {
                var emailUrl = Url.Action("Email", "TmkImage", new { area = "Trademark" });
                var parentId = attachments.First().DocParent;

                var newDocRespDocketingUrl = Url.Action("EmailDocRespDocketing", "TmkImage", new { area = "Trademark" });
                var reassignedDocRespDocketingUrl = Url.Action("EmailReassignedDocRespDocketing", "TmkImage", new { area = "Trademark" });
                
                var newDocRespReportingUrl = Url.Action("EmailDocRespReporting", "TmkImage", new { area = "Trademark" });
                var reassignedDocRespReportingUrl = Url.Action("EmailReassignedDocRespReporting", "TmkImage", new { area = "Trademark" });

                var workflows = await _docViewModelService.GenerateTrademarkWorkflow(attachments, parentId, isNewFileUpload, hasNewRespDocketing, hasRespDocketingReassigned, newDocRespDocketingUrl ?? "", reassignedDocRespDocketingUrl ?? "", hasNewRespReporting, hasRespReportingReassigned, newDocRespReportingUrl ?? "", reassignedDocRespReportingUrl ?? "");
                return new WorkflowHeaderViewModel { Id = parentId, EmailUrl = emailUrl, Workflows = workflows };
            }
            return new WorkflowHeaderViewModel();
        }

        // Removed during deep clean - GenerateGMWorkflow method
        // protected async Task<WorkflowHeaderViewModel> GenerateGMWorkflow(...) { ... }

        protected async Task<WorkflowHeaderViewModel> GenerateActionWorkflow(string systemTypeCode, List<WorkflowEmailAttachmentViewModel> attachments)
        {
            switch (systemTypeCode)
            {
                case SystemTypeCode.Patent:
                    var patSettings = await _patSettings.GetSetting();
                    if (patSettings.IsWorkflowOn)
                    {
                        var emailUrl = Url.Action("Email", "PatImageAct", new { area = "Patent" });
                        var parentId = attachments.First().DocParent;
                        var workflows = await _docViewModelService.GeneratePatentActionWorkflow(attachments, parentId);
                        if (workflows != null)
                            return new WorkflowHeaderViewModel { Id = parentId, EmailUrl = emailUrl, Workflows = workflows };
                    }
                    break;

                case SystemTypeCode.Trademark:
                    var tmkSettings = await _tmkSettings.GetSetting();
                    if (tmkSettings.IsWorkflowOn)
                    {
                        var emailUrl = Url.Action("Email", "TmkImageAct", new { area = "Trademark" });
                        var parentId = attachments.First().DocParent;
                        var workflows = await _docViewModelService.GenerateTrademarkActionWorkflow(attachments, parentId);
                        if (workflows != null)
                            return new WorkflowHeaderViewModel { Id = parentId, EmailUrl = emailUrl, Workflows = workflows };
                    }
                    break;

                // Removed during deep clean - GeneralMatter action workflow
                // case SystemTypeCode.GeneralMatter:
                //     var gmSettings = await _gmSettings.GetSetting();
                //     if (gmSettings.IsWorkflowOn) { ... }
                //     break;
            }
            return new WorkflowHeaderViewModel();
        }

        public async Task<List<WorkflowEmailViewModel>> ProcessDocVerificationNewActWorkflow(int docId = 0, string driveItemId = "")
        {
            var emailWorkflows = new List<WorkflowEmailViewModel>();

            if (!string.IsNullOrEmpty(driveItemId))
            {
                var tempDoc = await _docViewModelService.GetDocumentByDriveItemId(driveItemId);
                if (tempDoc != null) docId = tempDoc.DocId;
            }

            if (docId > 0)
            {
                emailWorkflows = await _docViewModelService.ProcessDocVerificationNewActWorkflow(docId,
                    Url.Action("EmailSaveWorkflow", "ActionDue", new { area = "Patent" }) ?? "",
                    Url.Action("EmailSaveWorkflow", "ActionDue", new { area = "Trademark" }) ?? "",
                    Url.Action("EmailSaveWorkflow", "ActionDue", new { area = "GeneralMatter" }) ?? "");
            }               

            return emailWorkflows;
        }
        #endregion

        protected abstract Task<IActionResult> SaveDroppedEmails(IEnumerable<IFormFile> droppedFiles, string documentLink, string folderId, string? roleLink, List<string>? responsibles);
        public abstract Task<IActionResult> SaveDroppedDocVerification(IEnumerable<IFormFile> droppedFiles);

        public async Task<IActionResult> MarkEPOCommunicationAsHandled(List<string>? communicationIds = null)
        {
            try
            {                
                if (communicationIds != null && communicationIds.Count > 0)
                {
                    // EPO service removed during debloat
                    var handledIds = new List<string>();

                    var communications = await _epoCommunicationService.QueryableList
                                            .Where(d => !string.IsNullOrEmpty(d.CommunicationId) && handledIds.Contains(d.CommunicationId))
                                            .ToListAsync();

                    if (communications != null && communications.Count > 0)
                    {
                        var userName = User.GetUserName();
                        communications.ForEach(d => { d.Handled = true; d.UpdatedBy = userName; });
                        await _epoCommunicationService.Update(communications);
                    }
                }                
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok();
        }
    }
}
