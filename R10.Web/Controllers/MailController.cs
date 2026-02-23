using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Graph;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;
using R10.Web.Extensions.ActionResults;
using R10.Web.Models;
using R10.Web.Models.MailViewModels;
using R10.Web.Models.PageViewModels;
using R10.Web.Services;
using R10.Web.Helpers;
using R10.Web.Security;
using R10.Core.Interfaces;
using R10.Web.Filters;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using R10.Core.Entities.MailDownload;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using R10.Web.Areas;
using DocumentFormat.OpenXml.Presentation;
using Microsoft.IdentityModel.Tokens;
using R10.Web.Services.MailDownload;
using R10.Core.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using System.Linq;

namespace R10.Web.Controllers
{
    [Authorize(Policy = SharedAuthorizationPolicy.CanAccessMail)]
    public class MailController : BaseController
    {
        private readonly GraphSettings _graphSettings;
        private readonly IParentEntityService<MailDownloadRule, MailDownloadRuleCondition> _ruleService;
        private readonly IParentEntityService<MailDownloadAction, MailDownloadActionFilter> _actionService;
        private readonly IBaseService<MailDownloadRuleResponsible> _ruleResponsibleService;
        private readonly IMailDownloadService _mailDownloadService;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly ILogger<MailController> _logger;

        public MailController(
            IOptions<GraphSettings> graphSettings,
            IParentEntityService<MailDownloadRule, MailDownloadRuleCondition> ruleService,
            IParentEntityService<MailDownloadAction, MailDownloadActionFilter> actionService,
            IBaseService<MailDownloadRuleResponsible> ruleResponsibleService,
            IMailDownloadService mailDownloadService,
            IStringLocalizer<SharedResource> localizer,
            ILogger<MailController> logger)
        {
            _graphSettings = graphSettings.Value;
            _ruleService = ruleService;
            _actionService = actionService;
            _ruleResponsibleService = ruleResponsibleService;
            _mailDownloadService = mailDownloadService;
            _localizer = localizer;
            _logger = logger;
        }

        [ServiceFilter(typeof(MailAuthorizationFilter))]
        public async Task<IActionResult> Index(string mailbox)
        {
            var mailSettings = _graphSettings.GetMailSettings(mailbox);
            var title = _localizer[mailSettings.MailboxName].ToString();
            var model = new PageViewModel()
            {
                Page = PageType.Search,
                PageId = "mailSearch",
                Title = title
            };

            var sidebarModel = new SidebarPageViewModel()
            {
                Title = title,
                PageTitle = model.Title,
                PageId = model.PageId,
                MainPartialView = "_SidebarSearchResultsPage",
                MainViewModel = model,
                SideBarPartialView = "_SidebarNav",
                SideBarViewModel = mailSettings.MailboxName 
            };

            if (Request.IsAjax())
                return PartialView("Index", sidebarModel);

            return View(sidebarModel);
        }

        [HttpGet]
        public IActionResult Mailbox(string id)
        {
            return RedirectToAction("Index", new { mailbox = id });
        }

        [HttpGet]
        public IActionResult Search(string id)
        {
            return RedirectToAction("Index", new { mailbox = id });
        }

        public async Task<IActionResult> PageRead([DataSourceRequest] DataSourceRequest request, List<QueryFilterViewModel> mainSearchFilters)
        {
            if (ModelState.IsValid)
            {
                var mailbox = mainSearchFilters.GetMailboxName();
                var msgs = await _mailDownloadService.GetGraphClient(mailbox).GetMailListViewModel(request, mainSearchFilters);
                var internetIds = msgs.Page.Select(m => m.InternetMessageId).ToList();
                var downloadedMsgs = await _mailDownloadService.DownloadLogDetailList.Where(l => internetIds.Contains(l.MailId)).Select(l => l.MailId).ToListAsync();
                foreach(var msg in msgs.Page)
                {
                    msg.IsDownloaded = downloadedMsgs.Contains(msg.InternetMessageId);
                }
                return Json(new DataSourceResult() { Data = msgs.Page, Total = msgs.Count });
            }
            return new JsonBadRequest(new { errors = ModelState.Errors() });
        }

        [ServiceFilter(typeof(MailAuthorizationFilter))]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetMessage(string id, string mailbox)
        {
            var graphClient = _mailDownloadService.GetGraphClient(mailbox);
            var message = await graphClient.GetMessage(id);
            var parentFolder = await graphClient.GetMailFolder(message.ParentFolderId);
            var mainFolder = await graphClient.GetParentFolder(message.ParentFolderId);

            ViewData["ParentFolder"] = parentFolder.DisplayName;
            ViewData["MainFolder"] = mainFolder.DisplayName;

            //uncomment to limit showing of case record links to DownloadedItemsFolder and its subfolders
            //if (mainFolder.DisplayName == MailGraphService.DownloadedItemsFolder)
            //{
                var documentLinks = await _mailDownloadService.GetDocumentLinks(message.InternetMessageId);
                if(documentLinks.Count() > 0)
                {
                    var documentLink = documentLinks.First();
                    ViewData["DocumentLinkUrl"] = GetDocumentLinkUrl(documentLink);
                    //uncomment to show View Downloaded Document link
                    //ViewData["DocumentPreviewUrl"] = await GetDocumentPreviewUrl(documentLink, message.GetDownloadFileName());
                }
            //}

            return PartialView("_Message", message);
        }

        private async Task<string?> GetDocumentPreviewUrl(string? documentLink, string userFileName)
        {
            if (string.IsNullOrEmpty(documentLink))
                return string.Empty;

            var docFileName = await _mailDownloadService.GetDocFileName(userFileName.ReplaceInvalidFilenameChars());
            if (string.IsNullOrEmpty(docFileName))
                    return string.Empty;

            var link = documentLink.Split('|');
            return Url.Action("PreviewDocument", "DocViewer", new
            {
                area = "Shared",
                system = link[0],
                screenCode = link[1],
                docFileName = docFileName,
                key = link[3]
            });
        }

        private string? GetDocumentLinkUrl(string? documentLink)
        {
            if (!string.IsNullOrEmpty(documentLink))
            {
                var link = documentLink.Split('|');

                switch (link[1].ToLower())
                {
                    case "inv":
                        return Url.Action("Detail", "Invention", new { area = "Patent", id = link[3], tab = "inventionDetailDocumentsTab" });
                    case "ca":
                        return Url.Action("Detail", "CountryApplication", new { area = "Patent", id = link[3], tab = "countryAppDocumentsTab" });
                    case "tmk":
                        return Url.Action("Detail", "TmkTrademark", new { area = "Trademark", id = link[3], tab = "trademarkDetailDocumentsTab" });
                    case "gm":
                        return Url.Action("Detail", "Matter", new { area = "GeneralMatter", id = link[3], tab = "matterDetailDocumentsTab" });
                }
            }

            return string.Empty;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetAttachment([FromBody] MailAttachment criteria)
        {
            var graphClient = _mailDownloadService.GetGraphClient(criteria.MailboxName);
            var attachment = await graphClient.GetAttachment(criteria.MessageId, criteria.Id);

            if (!(attachment.IsInline ?? false) && (attachment is FileAttachment))
                return new FileContentResult((attachment as FileAttachment).ContentBytes, attachment.GetContentType());

            return BadRequest("Not a file attachment");
        }

        [ServiceFilter(typeof(MailAuthorizationFilter))]
        public async Task<IActionResult> GetUnreadItemCount(string mailbox)
        {
            var graphClient = _mailDownloadService.GetGraphClient(mailbox);
            var mailFolders = await graphClient.GetMailFolders();
            var unreadItemCount = mailFolders.Flatten().Select(f => new MailUnreadItemCount() { Folder = f.Id, UnreadItemCount = f.UnreadItemCount }).ToList();

            return Json(unreadItemCount);
        }

        [ServiceFilter(typeof(MailAuthorizationFilter))]
        public async Task<IActionResult> GetMailCount(string mailbox)
        {
            var graphClient = _mailDownloadService.GetGraphClient(mailbox);
            var mailFolders = await graphClient.GetMailFolders();
            var count = 0;

            foreach(var mailFolder in mailFolders)
            {
                if (_graphSettings.GetMailSettings(mailbox).UnreadCountFolders.Contains(mailFolder.DisplayName))
                    count = count + GetMailCount(mailFolder);
            }

            return Json(new { UnreadItemCount = count });
        }

        private int GetMailCount(MailFolder folder)
        {
            var unreadItemCount = (folder.UnreadItemCount ?? 0);

            if (folder.ChildFolders != null)
                foreach (var childFolder in folder.ChildFolders)
                {
                    unreadItemCount = unreadItemCount + GetMailCount(childFolder);
                }

            return unreadItemCount;

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateIsRead(string id, bool isRead, string mailbox)
        {
            var graphClient = _mailDownloadService.GetGraphClient(mailbox);
            await graphClient.UpdateIsRead(id, isRead);
            return Ok();
        }

        [ServiceFilter(typeof(MailAuthorizationFilter))]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMessage(string id, string mailbox)
        {
            var graphClient = _mailDownloadService.GetGraphClient(mailbox);
            var message = await graphClient.GetMessage(id);
            var deletedItems = await graphClient.GetMailFolder("DeletedItems");

            if (message.ParentFolderId == deletedItems.Id)
                await graphClient.DeleteMessage(id);
            else
                await graphClient.MoveMessage(id, "DeletedItems");

            _logger.LogInformation("Mail item {messageId} with subject {subject} was deleted from {parentFolder} by {user}.", message.Id, message.Subject, message.ParentFolderId, User.GetUserName());

            return Ok();
        }

        [ServiceFilter(typeof(MailAuthorizationFilter))]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreMessage(string id, string mailbox)
        {
            var graphClient = _mailDownloadService.GetGraphClient(mailbox);
            await graphClient.RestoreMessage(id);
            _logger.LogInformation("Mail item {messageId} was restored by {user}.", id, User.GetUserName());
            return Ok();
        }

        [ServiceFilter(typeof(MailAuthorizationFilter))]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BatchUpdateIsRead(string[] ids, bool isRead, string mailbox)
        {
            var graphClient = _mailDownloadService.GetGraphClient(mailbox);
            foreach(var id in ids)
            {
                await graphClient.UpdateIsRead(id, isRead);
            }
            return Ok();
        }

        [ServiceFilter(typeof(MailAuthorizationFilter))]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BatchDeleteMessages(string[] ids, string mailbox)
        {
            var graphClient = _mailDownloadService.GetGraphClient(mailbox);
            var message = await graphClient.GetMessage(ids[0]);
            var deletedItems = await graphClient.GetMailFolder("DeletedItems");

            foreach(var id in ids)
            {
                if (message.ParentFolderId == deletedItems.Id)
                    await graphClient.DeleteMessage(id);
                else
                    await graphClient.MoveMessage(id, "DeletedItems");

                _logger.LogInformation("Mail item {messageId} was deleted from {parentFolder} by {user}.", id, message.ParentFolderId, User.GetUserName());
            }

            return Ok();
        }

        [ServiceFilter(typeof(MailAuthorizationFilter))]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BatchRestoreMessages(string[] ids, string mailbox)
        {
            var graphClient = _mailDownloadService.GetGraphClient(mailbox);

            foreach (var id in ids)
            {
                await graphClient.RestoreMessage(id);
                _logger.LogInformation("Mail item {messageId} was restored by {user}.", id, User.GetUserName());
            }

            return Ok();
        }

        [ServiceFilter(typeof(MailAuthorizationFilter))]
        [HttpGet]
        public async Task<IActionResult> GetEditor(string editor, string? id, string mailbox)
        {
            var graphClient = _mailDownloadService.GetGraphClient(mailbox);
            var model = new MailEditorViewModel();
            model.SendURL = Url.Action("Send"); //new message
            model.MailboxName = mailbox;

            if (!string.IsNullOrEmpty(id))
            {
                model.OriginalMessage = await graphClient.GetMessage(id);

                switch (editor)
                {
                    case MailEditor.Reply:
                        model.Id = model.OriginalMessage.Id;
                        model.Subject = model.OriginalMessage.Subject;
                        model.ToRecipients = string.Join(", ", ReplyTo(model.OriginalMessage, mailbox));
                        model.SendURL = Url.Action("Reply");
                        break;

                    case MailEditor.ReplyAll:
                        var toRecipients = ReplyTo(model.OriginalMessage, mailbox);
                        toRecipients.AddRange(model.OriginalMessage.ToRecipients.Where(r => r.EmailAddress.Address.ToLower() != _graphSettings.GetMailSettings(mailbox).User.ToLower()).Select(r => r.EmailAddress.ToAddress()).ToList());

                        model.Id = model.OriginalMessage.Id;
                        model.Subject = model.OriginalMessage.Subject;
                        model.ToRecipients = String.Join(", ", toRecipients.Distinct()) ;
                        model.CcRecipients = String.Join(", ", model.OriginalMessage.CcRecipients.Select(r => r.EmailAddress.ToAddress()).Distinct());
                        model.BccRecipients = String.Join(", ", model.OriginalMessage.BccRecipients.Select(r => r.EmailAddress.ToAddress()).Distinct());
                        model.SendURL = Url.Action("Reply");
                        break;

                    case MailEditor.Forward:
                        model.Id = model.OriginalMessage.Id;
                        model.Subject = model.OriginalMessage.Subject;
                        model.SendURL = Url.Action("Forward");
                        //todo: load attachments
                        //https://stackoverflow.com/questions/1696877/how-to-set-a-value-to-a-file-input-in-html/70485949#70485949
                        break;
                }
            }

            return PartialView("_Editor", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reply([FromBody] MailEditorViewModel data)
        {
            if (!ModelState.IsValid)
                return new JsonBadRequest(new { errors = ModelState.Errors() });

            var graphClient = _mailDownloadService.GetGraphClient(data.MailboxName);
            await graphClient.Reply(data.Id, NewMessage(data), GetBodyContent(data.Body));
            return Ok();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Forward([FromBody] MailEditorViewModel data)
        {
            if (!ModelState.IsValid)
                return new JsonBadRequest(new { errors = ModelState.Errors() });

            var message = NewMessage(data);
            var toRecipients = message.ToRecipients;
            message.ToRecipients = null;

            var graphClient = _mailDownloadService.GetGraphClient(data.MailboxName);
            await graphClient.Forward(data.Id, toRecipients, message, GetBodyContent(data.Body));
            return Ok();
        }

        [ServiceFilter(typeof(MailAuthorizationFilter))]
        public async Task<IActionResult> Download(string id, string mailbox)
        {
            var graphClient = _mailDownloadService.GetGraphClient(mailbox);
            var stream = await graphClient.Download(id);

            if (stream == null)
                return NotFound(); // returns a NotFoundResult with Status404NotFound response.

            return File(stream, "application/octet-stream"); // returns a FileStreamResult
        }

        [ServiceFilter(typeof(MailAuthorizationFilter))]
        [HttpGet]
        public async Task<IActionResult> GetRules(string mailbox)
        {
            var mailboxId = _graphSettings.GetMailboxId(mailbox);
            var rules = await _ruleService.QueryableList.Where(r => r.MailboxId == mailboxId).OrderBy(r => r.OrderOfEntry).Include(r => r.RuleConditions).Include(r => r.Action).Include(r => r.Responsibles).ToListAsync();
            return PartialView("_Rules", rules);
        }

        [HttpGet]
        public async Task<IActionResult> EditRule(int id)
        {
            var rule = await _ruleService.QueryableList.Where(r => r.Id == id).Include(r => r.RuleConditions).Include(r => r.Action).Include(r => r.Responsibles).FirstOrDefaultAsync();
            return PartialView("_RulesEditor", rule ?? new MailDownloadRule()
            {
                RuleConditions = new List<MailDownloadRuleCondition>()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MoveRuleUp(int id, string tStamp)
        {
            var rule = await _ruleService.GetByIdAsync(id);
            if (rule != null && rule.OrderOfEntry > 0)
            {
                var rules = new List<MailDownloadRule>();
                rules = await _ruleService.QueryableList.Where(r => r.MailboxId == rule.MailboxId && r.OrderOfEntry >= rule.OrderOfEntry - 1 && r.OrderOfEntry < rule.OrderOfEntry).ToListAsync();
                rules.ForEach(m => m.OrderOfEntry = m.OrderOfEntry + 1);

                UpdateEntityStamps(rule, id);
                rule.OrderOfEntry = rule.OrderOfEntry - 1;
                rule.tStamp = Convert.FromBase64String(tStamp ?? "");

                rules.Add(rule);
                await _ruleService.Update(rules);
            }

            return Ok();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MoveRuleDown(int id, string tStamp)
        {
            var rule = await _ruleService.GetByIdAsync(id);
            if (rule != null)
            {
                var rules = new List<MailDownloadRule>();
                rules = await _ruleService.QueryableList.Where(r => r.MailboxId == rule.MailboxId && r.OrderOfEntry <= rule.OrderOfEntry + 1 && r.OrderOfEntry > rule.OrderOfEntry).ToListAsync();
                rules.ForEach(r => r.OrderOfEntry = r.OrderOfEntry - 1);

                UpdateEntityStamps(rule, id);
                rule.OrderOfEntry = rule.OrderOfEntry + 1;
                rule.tStamp = Convert.FromBase64String(tStamp ?? "");

                rules.Add(rule);
                await _ruleService.Update(rules);
            }

            return Ok();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRuleStatus(int id, bool enabled, string tStamp)
        {
            var rule = await _ruleService.GetByIdAsync(id);
            if (rule != null)
            {
                UpdateEntityStamps(rule, id);
                rule.Enabled = enabled;
                rule.tStamp = Convert.FromBase64String(tStamp ?? "");

                await _ruleService.Update(rule);
            }

            return Ok();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRule(int id, string tStamp)
        {
            var rule = await _ruleService.GetByIdAsync(id);
            if (rule != null)
            {
                rule.tStamp = Convert.FromBase64String(tStamp ?? "");

                await _ruleService.Delete(rule);

                _logger.LogInformation("Mail rule {ruleName} was deleted by {user}.", rule.Name, User.GetUserName());
            }

            return Ok();
        }

        [ServiceFilter(typeof(MailAuthorizationFilter))]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveRule(string mailbox, MailDownloadRuleViewModel downloadRule)
        {
            var mailboxId = _graphSettings.GetMailboxId(mailbox);
            var rule = downloadRule.Id == 0 ? new MailDownloadRule() : await _ruleService.GetByIdAsync(downloadRule.Id);
            
            if (rule != null && downloadRule.RuleConditions != null)
            {
                downloadRule.RuleConditions.RemoveAll(c => c.Condition == MailDownloadCondition.None);

                rule.Id = downloadRule.Id;
                rule.Name = downloadRule.Name;
                rule.ActionId = downloadRule.ActionId;
                rule.StopProcessing = downloadRule.StopProcessing;
                rule.DoNotMove = downloadRule.DoNotMove;
                rule.DownloadFolderId = downloadRule.DownloadFolderId;
                rule.DownloadAttachments = downloadRule.DownloadAttachments;
                rule.tStamp = string.IsNullOrEmpty(downloadRule.tStamp) ? null : Convert.FromBase64String(downloadRule.tStamp);

                UpdateEntityStamps(rule, rule.Id);

                if (downloadRule.Id == 0)
                {
                    rule.OrderOfEntry = (await _ruleService.QueryableList.Where(r => r.MailboxId == mailboxId).MaxAsync(r => (int?)r.OrderOfEntry) ?? -1) + 1;
                    rule.MailboxId = mailboxId;
                    await _ruleService.Add(rule);
                }
                else
                {
                    await _ruleService.Update(rule);

                    var deleted = await _ruleService.ChildService.QueryableList.Where(d => d.RuleId == downloadRule.Id && !downloadRule.RuleConditions.Select(c => c.Id).Any(cId => cId == d.Id)).ToListAsync();
                    var updatedIds = downloadRule.RuleConditions.Where(c => c.Id > 0).Select(c => c.Id).ToList();
                    var updated = await _ruleService.ChildService.QueryableList.Where(d => d.RuleId == downloadRule.Id && updatedIds.Any(cId => cId == d.Id)).ToListAsync();
                    updated.ForEach(c =>
                    {
                        var condition = downloadRule.RuleConditions.Where(rc => rc.Id == c.Id).FirstOrDefault();

                        if (condition?.Condition != null)
                            c.Condition = condition.Condition;

                        c.Value = condition?.Value;
                        c.tStamp = string.IsNullOrEmpty(condition?.tStamp) ? null : Convert.FromBase64String(condition.tStamp);

                        UpdateEntityStamps(c, c.Id);
                    });

                    await _ruleService.ChildService.Delete(deleted);
                    await _ruleService.ChildService.Update(updated);
                }

                var added = downloadRule.RuleConditions.Where(c => c.Id == 0).Select(c => new MailDownloadRuleCondition()
                {
                    Id = c.Id,
                    RuleId = rule.Id,
                    Condition = c.Condition,
                    Value = c.Value,
                    tStamp = string.IsNullOrEmpty(c.tStamp) ? null : Convert.FromBase64String(c.tStamp)
                }).ToList();
                added.ForEach(c =>
                {
                    UpdateEntityStamps(c, c.Id);
                });
                await _ruleService.ChildService.Add(added);

                var oldResp = await _ruleResponsibleService.QueryableList.Where(r => r.RuleId == rule.Id).ToListAsync();
                if (downloadRule.Responsibles != null || oldResp != null)
                {
                    var responsibles = new List<MailDownloadRuleResponsible>();

                    if (downloadRule.Responsibles != null && downloadRule.Responsibles.Any())
                        responsibles = downloadRule.Responsibles.Select(responsible => new MailDownloadRuleResponsible()
                        {
                            RuleId = rule.Id,
                            Responsible = responsible,
                            DateCreated = rule.LastUpdate,
                            CreatedBy = rule.UpdatedBy,
                            LastUpdate = rule.LastUpdate,
                            UpdatedBy = rule.UpdatedBy
                        }).ToList();

                    var deletedResp = oldResp.Where(o => !responsibles.Any(r => r.Responsible == o.Responsible)).ToList();
                    var addedResp = responsibles.Where(r => !oldResp.Any(o => o.Responsible == r.Responsible)).ToList();

                    if (deletedResp.Any())
                        await _ruleResponsibleService.Delete(deletedResp);

                    if (addedResp.Any())
                        await _ruleResponsibleService.Add(addedResp);
                }
            }

            return Ok();
        }

        //todo: move to action controller
        public async Task<IActionResult> GetRuleActions()
        {
            var actions = await _actionService.QueryableList.Select(a => new SelectListItem() 
            { 
                Text = a.Name,
                Value = a.Id.ToString()
            }).ToListAsync();

            actions.Insert(0, new SelectListItem() { Text = "", Value = "" });
            return Json(actions);
        }

        [ServiceFilter(typeof(MailAuthorizationFilter))]
        //If the original message specifies a recipient in the replyTo property, per Internet Message Format(RFC 2822),
        //send the reply to the recipients in replyTo and not the recipient in the from property.
        private List<string> ReplyTo(Message message, string mailbox)
        {
            if (message.ReplyTo.Any())
                return message.ToRecipients.Where(r => r.EmailAddress.Address.ToLower() != _graphSettings.GetMailSettings(mailbox).User.ToLower()).Select(r => r.EmailAddress.ToAddress()).Distinct().ToList();
            else
                return new List<string>() { message.From.EmailAddress.ToAddress() };
        }

        private Message NewMessage(MailEditorViewModel data)
        {
            var message = new Message
            {
                ToRecipients = GetRecipients(data.ToRecipients),
                CcRecipients = GetRecipients(data.CcRecipients),
                BccRecipients = GetRecipients(data.BccRecipients)
            };

            //todo: attachments
            //https://stackoverflow.com/questions/42374501/microsoft-graph-send-mail-with-attachment
            //https://www.codeproject.com/Articles/5166075/Implementing-Batch-Mode-using-Kendo-Upload-Control
            //https://stackoverflow.com/questions/1696877/how-to-set-a-value-to-a-file-input-in-html/70485949#70485949

            return message;
        }

        private List<Recipient> GetRecipients(string? recipients)
        {
            var recipientList = new List<Recipient>();

            if (!string.IsNullOrEmpty(recipients))
            {
                foreach (var recipient in recipients.Replace(";", ",").Split(",", StringSplitOptions.RemoveEmptyEntries))
                {
                    var mailAddress = new MailAddress(recipient);
                    recipientList.Add(new Recipient()
                    {
                        EmailAddress = new EmailAddress()
                        {
                            Name = mailAddress.DisplayName,
                            Address = mailAddress.Address
                        }
                    });
                }
            }

            return recipientList;
        }

        private string GetBodyContent(string body)
        {
            return body.Replace("&lt;", "<").Replace("&gt;", ">").Replace("&amp;", "&");
        }

        [ServiceFilter(typeof(MailAuthorizationFilter))]
        public async Task<PartialViewResult> GetMailFolders(string activeFolderId, string mailbox)
        {
            var graphClient = _mailDownloadService.GetGraphClient(mailbox);
            var mailFolders = await graphClient.GetMailFolders();
            var denyChildFolders = new List<string>();
            var mailSettings = _graphSettings.GetMailSettings(mailbox);

            if (mailSettings.DenyChildFolders != null)
                denyChildFolders = mailFolders.Where(f => mailSettings.DenyChildFolders.Contains(f.DisplayName)).Select(f => f.Id).ToList();

            ViewData.Model = mailFolders;
            ViewData["ActiveFolder"] = string.IsNullOrEmpty(activeFolderId) ? mailFolders.Where(f => f.DisplayName == MailGraphService.InboxFolder).Select(f => f.Id).FirstOrDefault() : activeFolderId;
            ViewData["DeletedItems"] = mailFolders.Where(f => f.DisplayName == MailGraphService.DeletedItemsFolder).Select(f => f.Id).FirstOrDefault();
            ViewData["SentItems"] = mailFolders.Where(f => f.DisplayName == MailGraphService.SentItemsFolder).Select(f => f.Id).FirstOrDefault();
            ViewData["DenyChildFolders"] = denyChildFolders;

            return new PartialViewResult()
            {
                ViewName = "_Folders",
                ViewData = ViewData,
                TempData = TempData
            };
        }

        [ServiceFilter(typeof(MailAuthorizationFilter))]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFolder(string id, string parentId, string mailbox)
        {
            var graphClient = _mailDownloadService.GetGraphClient(mailbox);
            var folder = await graphClient.GetMailFolder(id);
            var deletedItems = await graphClient.GetMailFolder("DeletedItems");

            if (parentId == deletedItems.Id)
                await graphClient.DeleteFolder(id);
            else
                await graphClient.MoveFolder(id, "DeletedItems");

            _logger.LogInformation("Mail folder {name} was deleted by {user}.", folder.DisplayName, User.GetUserName());

            return Ok();
        }

        [ServiceFilter(typeof(MailAuthorizationFilter))]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RenameFolder(string id, string name, string mailbox)
        {
            var graphClient = _mailDownloadService.GetGraphClient(mailbox);

            if (MailGraphService.MailFolders.Contains(name))
                return BadRequest(_localizer["Folder name '{0}' is not allowed", name].ToString());

            var folder = await graphClient.GetMailFolder(id);
            var oldName = folder.DisplayName;

            var parentFolder = await graphClient.GetMailFolder(folder.ParentFolderId, true);
            if (parentFolder.ChildFolders != null && parentFolder.ChildFolders.Any(f => f.DisplayName.Trim().ToLower() == name.Trim().ToLower()))
                return BadRequest($"A folder with the name '{name}' already exists here.");

            folder.DisplayName = name;
            await graphClient.UpdateFolder(folder);

            _logger.LogInformation("Mail folder {oldName} was renamed to {newName} by {user}.", oldName, name, User.GetUserName());

            return Ok();
        }

        [ServiceFilter(typeof(MailAuthorizationFilter))]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateFolder(string id, string name, string mailbox)
        {
            var graphClient = _mailDownloadService.GetGraphClient(mailbox);

            if (MailGraphService.MailFolders.Contains(name))
                return BadRequest(_localizer["Folder name '{0}' is not allowed", name].ToString());

            await graphClient.AddChildFolder(id, name);

            _logger.LogInformation("Mail folder {name} was created by {user}.", name, User.GetUserName());

            return Ok();
        }

        [ServiceFilter(typeof(MailAuthorizationFilter))]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MoveFolder(string id, string destinationId, string mailbox)
        {
            var graphClient = _mailDownloadService.GetGraphClient(mailbox);

            await graphClient.MoveFolder(id, destinationId);
            _logger.LogInformation("Mail folder {folderId} was moved to {destinationId} by {user}.", id, destinationId, User.GetUserName());

            return Ok();
        }

        [ServiceFilter(typeof(MailAuthorizationFilter))]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MoveMessages(string[] ids, string destinationId, string mailbox)
        {
            var graphClient = _mailDownloadService.GetGraphClient(mailbox);

            foreach (var id in ids)
            {
                await graphClient.MoveMessage(id, destinationId);
                _logger.LogInformation("Mail item {id} was moved to {destinationId} by {user}.", id, destinationId, User.GetUserName());
            }

            return Ok();
        }
        public async Task<IActionResult> GetResponsibleList(string property, string text, FilterType filterType)
        {
            var data = await _mailDownloadService.GetResponsibleList();

            return Json(data);
        }
    }
}