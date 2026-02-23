using DocumentFormat.OpenXml.Spreadsheet;
using Kendo.Mvc;
using Kendo.Mvc.Resources;
using Kendo.Mvc.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Graph;
using System.Text.Json;
using R10.Core.Entities.GeneralMatter;
using R10.Core.Entities.MailDownload;
using R10.Core.Entities.Patent;
using R10.Core.Entities.Trademark;
using R10.Core.Helpers;
using R10.Core.Interfaces;
using R10.Web.Areas.Shared.ViewModels;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using R10.Web.Extensions;
using R10.Web.Helpers;
using R10.Web.Models.MailViewModels;
using R10.Core.Entities.Documents;
using Microsoft.Extensions.Options;
using R10.Core.Identity;

namespace R10.Web.Services.MailDownload
{
    public class MailDownloadService : IMailDownloadService
    {
        protected char RecipientsSeparator = ';';
        protected string DataTag => "{{DATA}}";

        protected readonly ICPiDbContext _cpiDbContext;
        protected readonly ClaimsPrincipal _user;
        protected readonly IGraphServiceClientFactory _graphServiceClientFactory;
        protected readonly GraphSettings _graphSettings;

        public MailDownloadService(
            ICPiDbContext cpiDbContext, 
            ClaimsPrincipal user, 
            IGraphServiceClientFactory graphServiceClientFactory,
            IOptions<GraphSettings> graphSettings
            )
        {
            _cpiDbContext = cpiDbContext;
            _user = user;
            _graphServiceClientFactory = graphServiceClientFactory;
            _graphSettings = graphSettings.Value;
        }

        public IQueryable<MailDownloadLogDetail> DownloadLogDetailList => _cpiDbContext.GetRepository<MailDownloadLogDetail>().QueryableList;

        /// <summary>
        /// Get status of last download
        /// </summary>
        /// <param name="endExpired"></param>
        /// <returns></returns>
        public async Task<MailDownloadStatus> GetStatus(string mailbox, bool endExpired = false)
        {
            var status = new MailDownloadStatus();
            var now = DateTime.Now;
            var downloadLog = await _cpiDbContext.GetRepository<MailDownloadLog>().QueryableList
                                        .Where(l => l.MailboxName == mailbox && l.DownloadStart != null) //ignore manual (drag&drop) downloads
                                        .OrderByDescending(l => l.Id).FirstOrDefaultAsync();

            if (downloadLog != null && downloadLog.DownloadStart != null)
            {
                if (downloadLog.DownloadEnd == null)
                {
                    //job is expired if started over 1hr ago
                    if (((DateTime)downloadLog.DownloadStart).AddHours(1) < now)
                    {
                        if (endExpired)
                            await EndDownload(downloadLog); 
                        else
                            status.Status = MailDownloadStatusType.Expired;
                    }
                    else
                        status.Status = MailDownloadStatusType.Running;
                }

                status.ReceivedDateTimeFrom = downloadLog.ReceivedDateTimeFrom;
                status.ReceivedDateTimeTo = downloadLog.ReceivedDateTimeTo;
                status.RuleId = downloadLog.RuleId;
                status.LogId = downloadLog.Id;
            }

            return status;
        }

        public async Task<(List<MailDownloadJob> Jobs, int LogId)> StartDownload(string mailbox, int ruleId = 0)
        {
            var status = await GetStatus(mailbox, true);

            //Prevent running multiple downloads
            if (status.Status != MailDownloadStatusType.Completed)
                throw new Exception($"Unable to start mail download. Status is {status.Status.ToString()}");

            //Download by ruleId is not filtered by ReceivedDateTime
            var receivedDateTimeFrom = ruleId == 0 ? status.RuleId == 0 ? status.ReceivedDateTimeTo : await GetLastReceivedDateTimeTo(mailbox) : null;
            var now = DateTime.Now;
            var jobs = ruleId == 0 ? await GetDownloadJobs(mailbox, receivedDateTimeFrom, now) : await GetDownloadJobs(mailbox, ruleId);
            var logId = 0;

            if (jobs.Count > 0)
            {
                var userName = _user.GetUserName();
                var mailCount = 0;

                foreach (var job in jobs)
                {
                    mailCount = mailCount + job.Messages.Count();
                }

                var downloadLog = new MailDownloadLog()
                {
                    DownloadStart = now,
                    DateCreated = now,
                    MailCount = mailCount,
                    ReceivedDateTimeFrom = receivedDateTimeFrom,
                    ReceivedDateTimeTo = ruleId == 0 ? now : null,
                    RuleId = ruleId,
                    MailboxName = mailbox,
                    CreatedBy = userName,
                    LastUpdate = now,
                    UpdatedBy = userName
                };

                _cpiDbContext.GetRepository<MailDownloadLog>().Add(downloadLog);
                await _cpiDbContext.SaveChangesAsync();

                logId = downloadLog.Id;
            }

            return (Jobs: jobs, LogId: logId);
        }

        private async Task<DateTime?> GetLastReceivedDateTimeTo(string mailbox)
        {
            return await _cpiDbContext.GetRepository<MailDownloadLog>().QueryableList
                            .Where(l => l.MailboxName == mailbox && l.RuleId == 0 && l.DownloadStart != null)
                            .OrderByDescending(l => l.Id)
                            .Select(l => l.ReceivedDateTimeTo)
                            .FirstOrDefaultAsync();
        }

        public async Task EndDownload(int logId)
        {
            var downloadLog = await _cpiDbContext.GetRepository<MailDownloadLog>().GetByIdAsync(logId);

            if (downloadLog != null && downloadLog.DownloadEnd == null)
                await EndDownload(downloadLog);
        }

        private async Task EndDownload(MailDownloadLog dowloadLog)
        {
            var now = DateTime.Now;

            _cpiDbContext.GetRepository<MailDownloadLog>().Attach(dowloadLog);

            dowloadLog.DownloadEnd = now;
            dowloadLog.LastUpdate = now;
            dowloadLog.UpdatedBy = _user.GetUserName();

            await _cpiDbContext.SaveChangesAsync();
        }

        public async Task LogDownloadDetail(int logId, int actionId, int ruleId, string documentLink, Message message)
        {
            var now = DateTime.Now;
            var userName = _user.GetUserName();

            _cpiDbContext.GetRepository<MailDownloadLogDetail>().Add(new MailDownloadLogDetail()
            {
                LogId = logId,
                ActionId = actionId,
                RuleId = ruleId,
                DocumentLink = documentLink,
                MailId = message.InternetMessageId,
                MailFromAddress = message.From.EmailAddress.ToAddress(),
                MailToRecipients = String.Join(RecipientsSeparator, message.ToRecipients.Select(r => r.EmailAddress.ToAddress()).ToArray()),
                MailSubject = message.Subject,
                MailReceivedDate = ((DateTimeOffset)message.ReceivedDateTime).DateTime,
                HasAttachments = message.HasAttachments,
                DateCreated = now,
                CreatedBy = userName,
                LastUpdate = now,
                UpdatedBy = userName
            });

            await _cpiDbContext.SaveChangesAsync();
        }

        private async Task<List<MailDownloadJob>> GetDownloadJobs(string mailbox, int ruleId)
        {
            var mailboxId = _graphSettings.GetMailboxId(mailbox);
            var rules = await _cpiDbContext.GetRepository<MailDownloadRule>().QueryableList.Where(r => r.MailboxId == mailboxId && r.Enabled && r.Id == ruleId)
                                           .Include(r => r.RuleConditions)
                                           .Include(r => r.Responsibles)
                                           .Include(r => r.Action)
                                           .ToListAsync();
            return await GetDownloadJobs(mailbox, rules);
        }

        private async Task<List<MailDownloadJob>> GetDownloadJobs(string mailbox, DateTime? receivedDateTimeFrom, DateTime receivedDateTimeTo)
        {
            var mailboxId = _graphSettings.GetMailboxId(mailbox);
            var rules = await _cpiDbContext.GetRepository<MailDownloadRule>().QueryableList.Where(r => r.MailboxId == mailboxId && r.Enabled)
                                           .OrderBy(r => r.OrderOfEntry)
                                           .Include(r => r.RuleConditions)
                                           .Include(r => r.Responsibles)
                                           .Include(r => r.Action)
                                           .ToListAsync();
            return await GetDownloadJobs(mailbox, rules, receivedDateTimeFrom, receivedDateTimeTo);
        }

        private async Task<List<MailDownloadJob>> GetDownloadJobs(string mailbox, List<MailDownloadRule> rules, DateTime? receivedDateTimeFrom = null, DateTime? receivedDateTimeTo = null)
        {
            var jobs = new List<MailDownloadJob>();

            var graphClient = GetGraphClient(mailbox);
            var mailFolders = (await graphClient.GetMailFolders()).Flatten();

            foreach (var rule in rules.Where(r => r.MailboxId == _graphSettings.GetMailboxId(mailbox)).ToList())
            {
                if (rule.Action == null)
                    continue;

                var filters = GetMailFilters(rule.RuleConditions);

                if (receivedDateTimeFrom != null)
                    filters.Add(new QueryFilterViewModel()
                    {
                        Property = "ReceivedDateTimeFrom",
                        Value = ((DateTime)receivedDateTimeFrom).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssK")
                    });

                if (receivedDateTimeTo != null)
                    filters.Add(new QueryFilterViewModel()
                    {
                        Property = "ReceivedDateTimeTo",
                        Value = ((DateTime)receivedDateTimeTo).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssK")
                    });

                var pageSize = 100;
                var request = new Kendo.Mvc.UI.DataSourceRequest() 
                { 
                    Page = 1, 
                    PageSize = pageSize, 
                    Sorts = new List<SortDescriptor>() { new SortDescriptor() { Member = "ReceivedDateTime", SortDirection = ListSortDirection.Descending } }
                };
                var select = "id,from,toRecipients,subject,body,receivedDateTime,hasAttachments,internetMessageId";

                var found = await graphClient.GetMessages(request, filters, select);
                if (found.Count > 0)
                {
                    jobs.Add(new MailDownloadJob()
                    {
                        MailboxName = mailbox,
                        Messages = found.Page,
                        ActionId = rule.ActionId,
                        ActionType = rule.Action.ActionType,
                        RuleId = rule.Id,
                        DownloadAttachments = rule.DownloadAttachments,
                        StopProcessing = rule.StopProcessing,
                        DownloadFolderId = !string.IsNullOrEmpty(rule.DownloadFolderId) && mailFolders.Any(f => f.Id == rule.DownloadFolderId) ? rule.DownloadFolderId : string.Empty,
                        DoNotMove = rule.DoNotMove,
                        Responsibles = rule.Responsibles == null ? null : rule.Responsibles.Select(r => r.Responsible).ToList(),
                    });

                    if (found.Count > pageSize)
                    {
                        var maxCount = _graphSettings.GetMailSettings(mailbox).Download.MaxCount; //max number of emails to process
                        var pageCount = (int)Math.Ceiling((double)(found.Count < maxCount ? found.Count : maxCount) / pageSize);
                        for (var i = 2; i <= pageCount; i++)
                        {
                            request.Page = i;
                            found = await graphClient.GetMessages(request, filters);
                            jobs.Add(new MailDownloadJob()
                            {
                                MailboxName = mailbox,
                                Messages = found.Page,
                                ActionId = rule.ActionId,
                                ActionType = rule.Action.ActionType,
                                RuleId = rule.Id,
                                DownloadAttachments = rule.DownloadAttachments,
                                StopProcessing = rule.StopProcessing,
                                DownloadFolderId = !string.IsNullOrEmpty(rule.DownloadFolderId) && mailFolders.Any(f => f.Id == rule.DownloadFolderId) ? rule.DownloadFolderId : string.Empty,
                                DoNotMove = rule.DoNotMove,
                                Responsibles = rule.Responsibles == null ? null : rule.Responsibles.Select(r => r.Responsible).ToList(),
                            });
                        }
                    }
                }
            }

            return jobs;
        }

        private async Task<List<string>> GetDocumentLinks(MailDownloadActionType actionType, List<QueryFilterViewModel> searchFilters, string mailbox)
        {
            var separators = _graphSettings.GetMailSettings(mailbox).Download?.CaseNumberCountrySubCaseSeparators ?? new string[] { " - ", " -- ", " / ", " \\ ", "/", "\\", "-", "--", " " };
            if (searchFilters.Count > 0)
            {
                var ids = new List<int>();

                switch (actionType)
                {
                    case MailDownloadActionType.Invention:
                        ids = await _cpiDbContext.GetReadOnlyRepositoryAsync<Invention>().QueryableList
                                                 .AddCriteria(searchFilters)
                                                 .Select(d => d.InvId).ToListAsync();
                        return ids.Select(id => $"P|Inv|InvId|{id.ToString()}").ToList();

                    case MailDownloadActionType.CountryApplication:
                        ids = await _cpiDbContext.GetReadOnlyRepositoryAsync<CountryApplication>().QueryableList
                                                 .AddCriteria(searchFilters, separators)
                                                 .Select(d => d.AppId).ToListAsync();
                        return ids.Select(id => $"P|CA|AppId|{id.ToString()}").ToList();

                    case MailDownloadActionType.Trademark:
                        ids = await _cpiDbContext.GetReadOnlyRepositoryAsync<TmkTrademark>().QueryableList
                                                 .AddCriteria(searchFilters, separators)
                                                 .Select(d => d.TmkId).ToListAsync();
                        return ids.Select(id => $"T|Tmk|TmkId|{id.ToString()}").ToList();

                    case MailDownloadActionType.GeneralMatter:
                        ids = await _cpiDbContext.GetReadOnlyRepositoryAsync<GMMatter>().QueryableList
                                                 .AddCriteria(searchFilters)
                                                 .Select(d => d.MatId).ToListAsync();
                        return ids.Select(id => $"G|GM|MatId|{id.ToString()}").ToList();
                }
            }

            return new List<string>();
        }

        public async Task<List<string>> GetDocumentLinks(MailDownloadActionType actionType, Message message, List<MailDownloadFilter> downloadfilters, string mailbox)
        {
            var searchFilters = new List<QueryFilterViewModel>();
            foreach (var filter in downloadfilters)
            {
                var matches = GetDataMatches(message, filter.Patterns, filter.Length, filter.ValueHasNoSpace);

                //skip download if at least one filter has no match
                if (matches.Count == 0)
                    return new List<string>();

                searchFilters.Add(new QueryFilterViewModel()
                {
                    Property = filter.Name, // Use $"MultiSelect_{filter.Name}" ??
                    Value = matches.Count == 1 ? matches[0] : JsonSerializer.Serialize(matches)
                });
            }

            return await GetDocumentLinks(actionType, searchFilters, mailbox);
        }

        public async Task<List<string>> GetDocumentLinks(string mailId)
        {
            return await _cpiDbContext.GetReadOnlyRepositoryAsync<MailDownloadLogDetail>().QueryableList.Where(l => l.MailId == mailId).Select(l => l.DocumentLink).ToListAsync();
        }

        public async Task<List<MailDownloadFilter>> GetDownloadFilters(int actionId)
        {
            var maps = await _cpiDbContext.GetReadOnlyRepositoryAsync<MailDownloadDataMap>().QueryableList
                                          .Where(m => m.ActionFilters != null && m.ActionFilters.Any(f => f.ActionId == actionId))
                                          .Include(m => m.Attribute)
                                          .Include(m => m.MapPatterns)
                                          .ToListAsync();
            var filters = new List<MailDownloadFilter>();

            //skip action if at least one filter has no map pattern
            if (!maps.Any(m => m.Attribute == null || m.MapPatterns == null || m.MapPatterns.Count == 0))
            {
                foreach (var map in maps)
                {
                    var patterns = map.MapPatterns.Select(p => p.Pattern).ToList();
                    var filter = filters.Where(f => f.Name == map.Attribute.Name).FirstOrDefault();

                    if (filter == null)
                        filters.Add(new MailDownloadFilter()
                        {
                            Name = map.Attribute.Name,
                            Length = map.Attribute.Length,
                            ValueHasNoSpace = map.Attribute.ValueHasNoSpace,
                            Patterns = patterns
                        });
                    else
                        filter.Patterns.AddRange(patterns);
                }
            }

            return filters;
        }

        private List<QueryFilterViewModel> GetMailFilters(List<MailDownloadRuleCondition> ruleConditions)
        {
            var filters = new List<QueryFilterViewModel>();

            foreach (var condition in ruleConditions)
            {
                var value = (condition.Value ?? "").Replace("'", "''");

                switch (condition.Condition)
                {
                    case MailDownloadCondition.From:
                        filters.Add(new QueryFilterViewModel()
                        {
                            Property = "FromNameOrAddress",
                            Value = value
                        });
                        break;

                    case MailDownloadCondition.SubjectIncludes:
                        filters.Add(new QueryFilterViewModel()
                        {
                            Property = "Subject",
                            Value = value
                        });
                        break;

                    case MailDownloadCondition.BodyIncludes:
                        filters.Add(new QueryFilterViewModel()
                        {
                            Property = "Body",
                            Value = value
                        });
                        break;

                    case MailDownloadCondition.SubjectOrBodyIncludes:
                        filters.Add(new QueryFilterViewModel()
                        {
                            Property = "Keyword",
                            Value = value
                        });
                        break;
                }
            }

            return filters;
        }

        private List<string> GetDataMatches(List<string> contents, List<string> patterns, int length, bool? noSpace = false)
        {
            var matches = new List<string>();
            var matchTimeout = TimeSpan.FromSeconds(1);
            foreach (var pattern in patterns)
            {
                var literal = Regex.Match(pattern.Trim(), @"^\[\[(.*)\]\]$", RegexOptions.None, matchTimeout); //literal if enclosed in [[ ]]
                if (literal.Success)
                {
                    if (!matches.Contains(literal.Groups[1].Value))
                        matches.Add(literal.Groups[1].Value);

                    continue;
                }

                foreach (var content in contents)
                {
                    // get all possible matches in case the pattern has multiple instances
                    foreach (Match match in Regex.Matches(content, Regex.Escape(pattern).Replace(Regex.Escape(DataTag), $"(.{{1,{length.ToString()}}})"), RegexOptions.IgnoreCase, matchTimeout))
                    {
                        if (match.Success)
                        {
                            var values = match.Groups[1].Value.Trim().Split(" ");

                            if ((noSpace ?? false))
                                matches.Add(values[0]);
                            else
                            {
                                var temp = string.Empty;
                                foreach (var value in values)
                                {
                                    temp = string.IsNullOrEmpty(temp) ? value : $"{temp} {value}";
                                    if (!matches.Contains(temp))
                                        matches.Add(temp);
                                }
                            }
                        }
                    }
                }
            }

            return matches;
        }

        private List<string> GetDataMatches(string content, List<string> patterns, int length, bool? noSpace = false)
        {
            return GetDataMatches(new List<string>() { content }, patterns, length, noSpace);
        }

        public List<string> GetDataMatches(Message message, List<string> patterns, int length, bool? noSpace = false)
        {
            //replace html tags with space
            var content = Regex.Replace(message.Body.Content, @"<.*?>", " ");

            //replace non-breaking space markup with space
            content = content.Replace("&nbsp;", " ");

            //replace multiple spaces with one space
            //content = Regex.Replace(content, @"[ ]{2,}", " ");

            //replace multiple whitespaces with one space
            content = Regex.Replace(content, @"\s+", " ");

            return GetDataMatches(new List<string>() { message.Subject, content }, patterns, length, noSpace);
        }

        public async Task<IFormFile> DownloadAsFormFile(string messageId, string fileName, string mailbox)
        {
            var graphClient = GetGraphClient(mailbox);
            var stream = await graphClient.Download(messageId);

            return new FormFile(stream, 0, stream.Length, "droppedFiles", fileName.ReplaceInvalidFilenameChars())
            {
                Headers = new HeaderDictionary(),
                ContentType = "message/rfc822"
            };
        }

        public async Task<List<IFormFile>> DownloadAttachmentsAsFormFiles(string messageId, string mailbox)
        {
            var graphClient = GetGraphClient(mailbox);
            var files = new List<IFormFile>();
            var attachments = await graphClient.GetAttachments(messageId);

            foreach(var attachment in attachments)
            {
                if (!(attachment.IsInline ?? false) && (attachment is FileAttachment))
                {
                    var stream = new MemoryStream((attachment as FileAttachment).ContentBytes);
                    files.Add(new FormFile(stream, 0, stream.Length, "droppedFiles", attachment.Name.ReplaceInvalidFilenameChars())
                    {
                        Headers = new HeaderDictionary(),
                        ContentType = attachment.GetContentType()
                    });
                }
            }

            return files;
        }

        public async Task<bool> IsDownloaded(string mailId, string documentLink)
        {
            return await _cpiDbContext.GetReadOnlyRepositoryAsync<MailDownloadLogDetail>().QueryableList.AnyAsync(l => l.MailId == mailId && l.DocumentLink == documentLink);
        }

        public async Task<Message> GetMessage(string mailId)
        {
            var message = new Message();
            var logDetail = await _cpiDbContext.GetReadOnlyRepositoryAsync<MailDownloadLogDetail>().QueryableList.FirstOrDefaultAsync(l => l.MailId == mailId);

            if (logDetail != null)
            {
                var from = new MailAddress(logDetail.MailFromAddress);

                message.Id = logDetail.MailId;
                message.Body = new ItemBody() { Content = "" };
                message.Subject = logDetail.MailSubject;
                message.From = new Recipient() { EmailAddress = new EmailAddress() 
                                        { 
                                            Name = from.DisplayName, 
                                            Address = from.Address 
                                        } };
                message.ReceivedDateTime = logDetail.MailReceivedDate;
                message.ToRecipients = logDetail.MailToRecipients.Split(RecipientsSeparator)
                                        .Select(r => new MailAddress(r))
                                        .Select(r => new Recipient()
                                        {
                                            EmailAddress = new EmailAddress()
                                            {
                                                Name = r.DisplayName,
                                                Address = r.Address
                                            }
                                        })
                                        .ToList();
                message.CcRecipients = new List<Recipient>();
                message.BccRecipients = new List<Recipient>();
                message.HasAttachments = logDetail.HasAttachments;
            }

            return message;
        }

        public async Task<string?> GetDocFileName(string userFileName)
        {
            return await _cpiDbContext.GetReadOnlyRepositoryAsync<DocFile>().QueryableList
                            .Where(d => d.UserFileName == userFileName)
                            .Select(d => d.DocFileName)
                            .FirstOrDefaultAsync();
        }

        public async Task MoveDownloadedMessage(string messageId, string downloadedItemsFolderId, string mailbox)
        {
            if (!string.IsNullOrEmpty(downloadedItemsFolderId))
            {
                var graphClient = GetGraphClient(mailbox);
                await graphClient.MoveMessage(messageId, downloadedItemsFolderId);
            }
        }

        public async Task<MailFolder> GetDownloadedItemsFolder(string mailbox)
        {
            var graphClient = GetGraphClient(mailbox);
            var downloadedItemsFolder = (await graphClient.Me.MailFolders.Request().GetAsync()).FirstOrDefault(f => f.DisplayName == MailGraphService.DownloadedItemsFolder);

            if (downloadedItemsFolder == null)
                downloadedItemsFolder = await graphClient.AddFolder(MailGraphService.DownloadedItemsFolder);

            return downloadedItemsFolder;
        }

        public async Task<List<MailFolderViewModel>> GetDownloadFolders(string mailbox)
        {
            var graphClient = GetGraphClient(mailbox);
            var mailFolders = (await graphClient.GetMailFolders()).Where(f => f.DisplayName == MailGraphService.InboxFolder || f.DisplayName == MailGraphService.DownloadedItemsFolder).ToList().Flatten(true);
            var inboxFolderId = mailFolders.Where(f => f.DisplayName == MailGraphService.InboxFolder).Select(f => f.Id).FirstOrDefault();
            var defaultDownloadFolderId = mailFolders.Where(f => f.DisplayName == MailGraphService.DownloadedItemsFolder).Select(f => f.Id).FirstOrDefault();
            var downloadFolders = mailFolders.Where(f => f.DisplayName != MailGraphService.InboxFolder)
                .Select(f => new MailFolderViewModel()
                {
                    Id = f.Id,
                    DisplayName = f.DisplayName.Trim(),
                    Padding = f.ParentFolderId == inboxFolderId ? 0 : f.DisplayName.IndexOf(f.DisplayName.TrimStart()),
                    Icon = (MailGraphService.MailFolderIcons.ContainsKey(f.DisplayName) ? MailGraphService.MailFolderIcons[f.DisplayName] : "fad fa-folder").Replace("fad ", "fal ")
                })
                .ToList();

            return downloadFolders;
        }

        /// <summary>
        /// Log messages saved by drag and drop
        /// </summary>
        /// <param name="messageIds"></param>
        /// <param name="documentLink"></param>
        /// <returns></returns>
        public async Task LogDownloadedMessages(string[] messageIds, string documentLink, string mailbox)
        {
            var now = DateTime.Now;
            var userName = _user.GetUserName();
            var graphClient = GetGraphClient(mailbox);
            var logDetails = new List<MailDownloadLogDetail>();

            foreach(var messageId in messageIds)
            {
                var message = await graphClient.GetMessage(messageId);
                logDetails.Add(new MailDownloadLogDetail()
                {
                    ActionId = 0,
                    RuleId = 0,
                    DocumentLink = documentLink,
                    MailId = message.InternetMessageId,
                    MailFromAddress = message.From.EmailAddress.ToAddress(),
                    MailToRecipients = String.Join(RecipientsSeparator, message.ToRecipients.Select(r => r.EmailAddress.ToAddress()).ToArray()),
                    MailSubject = message.Subject,
                    MailReceivedDate = ((DateTimeOffset)message.ReceivedDateTime).DateTime,
                    HasAttachments = message.HasAttachments,
                    DateCreated = now,
                    CreatedBy = userName,
                    LastUpdate = now,
                    UpdatedBy = userName
                });
            }

            var downloadLog = new MailDownloadLog()
            {
                DownloadStart = null, //set DownloadStart date to null so log is ignored when checking download status
                DownloadEnd = now,
                MailCount = 1,
                RuleId = 0,
                DateCreated = now,
                CreatedBy = userName,
                LastUpdate = now,
                UpdatedBy = userName,
                LogDetails = logDetails
            };

            _cpiDbContext.GetRepository<MailDownloadLog>().Add(downloadLog);
            await _cpiDbContext.SaveChangesAsync();
        }

        public GraphServiceClient GetGraphClient(string mailbox)
        {
            var mailSettings = _graphSettings.GetMailSettings(mailbox);

            switch (mailSettings.GraphClientAuthentication)
            {
                //development use only
                case GraphClientAuthenticationFlow.Interactive:
                    return _graphServiceClientFactory.GetGraphClientInteractive(_graphSettings.Mail, _user.GetUserIdentifier());

                //development use only
                case GraphClientAuthenticationFlow.OnBehalfOf:
                    return _graphServiceClientFactory.GetGraphClientOnBehalfOf(_graphSettings.Mail, _user.GetUserIdentifier());

                //development use only
                case GraphClientAuthenticationFlow.AuthorizationCode:
                    return _graphServiceClientFactory.GetGraphClientByAuthorizationCode(_graphSettings.Mail, _user.GetUserIdentifier());

                //production always use ropc
                default:
                    return _graphServiceClientFactory.GetGraphClientByRopc(_graphSettings.Mail, mailSettings.User, mailSettings.Password.ToSecureString());
            }
        }

        public async Task<string> GetRoleLink(MailDownloadActionType actionType, string id)
        {
            string roleLink = "";

            switch (actionType)
            {
                case MailDownloadActionType.Invention:
                    roleLink = $"PI,{id}";
                    break;

                case MailDownloadActionType.CountryApplication:
                    var invId = await _cpiDbContext.GetReadOnlyRepositoryAsync<CountryApplication>().QueryableList
                                             .Where(ca => ca.AppId == int.Parse(id))
                                             .Select(ca => ca.InvId).FirstOrDefaultAsync();
                    roleLink = $"PI,{invId}|PC,{id}";
                    break;

                case MailDownloadActionType.Trademark:
                    roleLink = $"TM,{id}";
                    break;

                case MailDownloadActionType.GeneralMatter:
                    roleLink = $"GM,{id}";
                    break;
            }

            return roleLink;
        }

        public async Task<List<R10.Core.DTOs.LookupDTO>> GetResponsibleList()
        {
            var responsibleList = await _cpiDbContext.GetReadOnlyRepositoryAsync<CPiGroup>().QueryableList
                                                .Select(g => new R10.Core.DTOs.LookupDTO { Text = g.Name, Value = g.Id.ToString() })
                                                .Distinct().ToListAsync();

            responsibleList.AddRange(await _cpiDbContext.GetReadOnlyRepositoryAsync<CPiUser>().QueryableList
                                                .Select(u => new R10.Core.DTOs.LookupDTO { Text = u.FirstName + " " + u.LastName + " (" + u.Email + ")", Value = u.Id })
                                                .Distinct().ToListAsync());

            return responsibleList.Distinct().OrderBy(r => r.Text).ToList();
        }

        public async Task<string?> GetResponsibleName(string id)
        {
            var groupId = 0;

            if (int.TryParse(id, out groupId))
                return await _cpiDbContext.GetReadOnlyRepositoryAsync<CPiGroup>().QueryableList.Where(g => g.Id == groupId).Select(g => g.Name).FirstOrDefaultAsync();
            else
                return await _cpiDbContext.GetReadOnlyRepositoryAsync<CPiUser>().QueryableList.Where(u => u.Id == id).Select(u => u.FirstName + " " + u.LastName + " (" + u.Email + ")").FirstOrDefaultAsync();
        }

        public async Task<List<string>> GetResponsibleNames(List<string> ids)
        {
            var names = new List<string>();
            foreach (var id in ids)
            {
                var name = await GetResponsibleName(id);
                if (!string.IsNullOrEmpty(name))
                    names.Add(name);
            }

            return names;
        }
    }

    public interface IMailDownloadService
    {
        IQueryable<MailDownloadLogDetail> DownloadLogDetailList { get; }
        Task<MailDownloadStatus> GetStatus(string mailbox, bool endExpired = false);
        Task<(List<MailDownloadJob> Jobs, int LogId)> StartDownload(string mailbox, int ruleId = 0);
        Task EndDownload(int logId);
        Task LogDownloadDetail(int logId, int actionId, int ruleId, string documentLink, Message message);
        Task<List<MailDownloadFilter>> GetDownloadFilters(int actionId);
        List<string> GetDataMatches(Message message, List<string> patterns, int length, bool? noSpace = false);
        Task<List<string>> GetDocumentLinks(MailDownloadActionType actionType, Message message, List<MailDownloadFilter> downloadfilters, string mailbox);
        Task<List<string>> GetDocumentLinks(string mailId);
        Task<IFormFile> DownloadAsFormFile(string messageId, string fileName, string mailbox);
        Task<List<IFormFile>> DownloadAttachmentsAsFormFiles(string messageId, string mailbox);
        Task<bool> IsDownloaded(string mailId, string documentLink);
        Task<Message> GetMessage(string mailId);
        Task<string?> GetDocFileName(string userFileName);
        Task MoveDownloadedMessage(string messageId, string downloadedItemsFolderId, string mailbox);
        Task<MailFolder> GetDownloadedItemsFolder(string mailbox);
        Task<List<MailFolderViewModel>> GetDownloadFolders(string mailbox);
        Task LogDownloadedMessages(string[] messageId, string documentLink, string mailbox);

        GraphServiceClient GetGraphClient(string mailbox);

        /// <summary>
        /// For DocuSign
        /// </summary>
        /// <param name="actionType"></param>
        /// <param name="id"></param>
        /// <returns>
        /// MailDownloadActionType.Invention = PI,{invId}
        /// MailDownloadActionType.CountryApplication = PI,{invId}|PC,{appid}
        /// MailDownloadActionType.Trademark = TM,{tmkId}
        /// MailDownloadActionType.GeneralMatter = GM,{matId}
        /// </returns>
        Task<string> GetRoleLink(MailDownloadActionType actionType, string id);

        Task<List<R10.Core.DTOs.LookupDTO>> GetResponsibleList();
        Task<string?> GetResponsibleName(string id);
        Task<List<string>> GetResponsibleNames(List<string> ids);
    }
}
