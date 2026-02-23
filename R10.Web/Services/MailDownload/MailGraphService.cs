using Kendo.Mvc.UI;
using Microsoft.Graph;
using R10.Web.Areas.Shared.ViewModels;
using Microsoft.AspNetCore.StaticFiles;
using R10.Web.Models.MailViewModels;
using R10.Web.Helpers;
using Kendo.Mvc.Extensions;

namespace R10.Web.Services
{
    public static class MailGraphService
    {
        public static string InboxFolder => "Inbox";
        public static string SentItemsFolder => "Sent Items";
        public static string DeletedItemsFolder => "Deleted Items";
        public static string DownloadedItemsFolder => "Downloaded Items";
        public static Dictionary<string, string> MailFolderIcons =>
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { InboxFolder, "fad fa-inbox" },
                { SentItemsFolder, "fad fa-paper-plane" },
                { DeletedItemsFolder, "fad fa-trash" },
                { DownloadedItemsFolder, "fad fa-download" }
            };
        public static List<string> MailFolders => MailFolderIcons.Select(i => i.Key).ToList();
        public static string DefaultAttachmentIcon => "fal fa-file";
        public static Dictionary<string, string> AttachmentIcons =>
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { ".pdf", "far fa-file-pdf" },
                { ".doc", "fal fa-file-word" },
                { ".docx", "fal fa-file-word" },
                { ".bmp", "fal fa-image" },
                { ".gif", "fal fa-image" },
                { ".png", "fal fa-image" },
                { ".jpg", "fal fa-image" },
                { ".jpeg", "fal fa-image" },
                { ".tiff", "fal fa-image" },
                { ".svg", "fab fa-internet-explorer" },
                { ".xls", "fal fa-file-excel" },
                { ".xlsx", "fal fa-file-excel" },
                { ".ppt", "fal fa-file-powerpoint" },
                { ".pptx", "fal fa-file-powerpoint" },
                { ".eml", "fal fa-envelope" },
                { ".msg ", "fal fa-envelope" },
                { ".txt", "fal fa-file-alt" },
                { ".csv", "fal fa-file-csv" },
                { ".htm", "fal fa-file-code" },
                { ".html", "fal fa-file-code" },
                { ".ttf", "fal fa-font" },
                { ".zip ", "fal fa-file-archive" }
            };

        //https://developer.mozilla.org/en-US/docs/Web/HTTP/Basics_of_HTTP/MIME_types
        public static List<string> PreviewContentTypes => new List<string>()
        {
            "application/pdf",
            "text/html",
            "image/gif", "image/jpeg", "image/png"
        };

        public static string GetIcon(this Attachment attachment)
        {
            var icon = "";
            var extension = Path.GetExtension(attachment.Name);
            if (!string.IsNullOrEmpty(extension))
                AttachmentIcons.TryGetValue(extension, out icon);

            return string.IsNullOrEmpty(icon) ? DefaultAttachmentIcon : icon;
        }

        //Graph returns ContentType as application/octet-stream even for known mime types
        public static string GetContentType(this Attachment attachment)
        {
            string? contentType;
            new FileExtensionContentTypeProvider().TryGetContentType(attachment.Name, out contentType);
            return contentType ?? attachment.ContentType;
        }

        public static string ToAddress(this EmailAddress emailAddress)
        {
            if (string.IsNullOrEmpty(emailAddress.Name) || emailAddress.Name == emailAddress.Address)
                return emailAddress.Address;

            return $"{emailAddress.Name} <{emailAddress.Address}>";
        }

        public static bool HasPreview(this Attachment attachment)
        {
            string contentType = attachment.GetContentType();
            return PreviewContentTypes.Any(t => t.Equals(contentType, StringComparison.OrdinalIgnoreCase));
        }

        public static async Task<Attachment> GetAttachment(this GraphServiceClient graphClient, string messageId, string attachmentId)
        {
            return await graphClient.Me.Messages[messageId].Attachments[attachmentId].Request().GetAsync();
        }

        public static async Task<List<Attachment>> GetAttachments(this GraphServiceClient graphClient, string messageId)
        {
            return (await graphClient.Me.Messages[messageId].Attachments.Request().GetAsync()).ToList();
        }

        public static async Task UpdateIsRead(this GraphServiceClient graphClient, string messageId, bool isRead)
        {
            await graphClient.Me.Messages[messageId].Request().Select("IsRead").UpdateAsync(new Message { IsRead = isRead });
        }

        public static async Task MoveMessage(this GraphServiceClient graphClient, string messageId, string folderId)
        {
            await graphClient.Me.Messages[messageId].Move(folderId).Request().PostAsync();
        }

        public static async Task DeleteMessage(this GraphServiceClient graphClient, string messageId)
        {
            await graphClient.Me.Messages[messageId].Request().DeleteAsync();
        }

        public static async Task<MailFolder> AddFolder(this GraphServiceClient graphClient, string displayName, bool isHidden = false)
        {
            return await graphClient.Me.MailFolders.Request().AddAsync(new MailFolder()
            {
                DisplayName = displayName,
                IsHidden = isHidden
            });
        }

        public static async Task<MailFolder> AddChildFolder(this GraphServiceClient graphClient, string parentId, string displayName, bool isHidden = false)
        {
            return await graphClient.Me.MailFolders[parentId].ChildFolders.Request().AddAsync(new MailFolder()
            {
                DisplayName = displayName,
                IsHidden = isHidden
            });
        }

        public static async Task<MailFolder> UpdateFolder(this GraphServiceClient graphClient, MailFolder mailFolder)
        {
            return await graphClient.Me.MailFolders[mailFolder.Id].Request().UpdateAsync(mailFolder);
        }

        public static async Task MoveFolder(this GraphServiceClient graphClient, string folderId, string parentFolderId)
        {
            await graphClient.Me.MailFolders[folderId].Move(parentFolderId).Request().PostAsync();
        }

        public static async Task DeleteFolder(this GraphServiceClient graphClient, string folderId)
        {
            await graphClient.Me.MailFolders[folderId].Request().DeleteAsync();
        }

        //Graph has no restore method for messages
        //Use deleted item's LAPFID to get its original parent folder
        public static async Task RestoreMessage(this GraphServiceClient graphClient, string messageId)
        {
            var messageReq = graphClient.Me.MailFolders["recoverableitemsDeletions"].Messages[messageId].Request();
            var foldersReq = graphClient.Me.MailFolders.Request();

            //Include message's LAPFID (Last Active Parent FolderId)
            //https://techcommunity.microsoft.com/t5/exchange-team-blog/announcing-original-folder-item-recovery/ba-p/606833
            messageReq.QueryOptions.Add(new QueryOption("$expand", $"SingleValueExtendedProperties($filter=Id eq 'Binary 0x348A')"));
            messageReq.Select("id");

            //Include folder's PR_ENTRYID
            foldersReq.QueryOptions.Add(new QueryOption("$expand", $"SingleValueExtendedProperties($filter=Id eq 'Binary 0x0FFF')"));

            var message = await messageReq.GetAsync();
            var folders = await foldersReq.GetAsync();

            //Get folderId using lapFid
            //Extend property values (LAPFID and PR_ENTRYID) are binary encoded strings
            //Powershell example
            //https://github.com/gscales/Graph-Powershell-101-Binder/blob/master/Mailbox-Dumpster/Restoring%20a%20Items%20to%20where%20it%20was%20deleted%20from.md
            //https://github.com/gscales/Powershell-Scripts/blob/master/Graph101/Dumpster.ps1
            var folderId = "";
            var lapFid = BitConverter.ToString(Convert.FromBase64String(message.SingleValueExtendedProperties.FirstOrDefault()?.Value ?? "")).Replace("-", "");

            //Get all folders and all child folders
            var folderIds = folders.ToList().ToDictionary(f => f.Id, f => BitConverter.ToString(Convert.FromBase64String(f.SingleValueExtendedProperties.FirstOrDefault()?.Value ?? "")).Replace("-", ""));
            foreach (var folder in folders)
            {
                if (folder.ChildFolderCount > 0)
                    folderIds.AddRange(await graphClient.GetChildFolderIds(folder.Id));
            }

            foreach (var id in folderIds)
            {
                //lapFid is part of entryId
                if (id.Value.Contains(lapFid))
                {
                    folderId = id.Key;
                    break;
                }
            }

            //Unable to resolve folderId
            if (string.IsNullOrEmpty(folderId))
                throw new Exception("Unable to restore message.");

            //Move message to original folder using folderId
            await graphClient.MoveMessage(messageId, folderId);
        }

        public static async Task<Dictionary<string, string>> GetChildFolderIds(this GraphServiceClient graphClient, string folderId)
        {
            var childFoldersReq = graphClient.Me.MailFolders[folderId].ChildFolders.Request();
            childFoldersReq.QueryOptions.Add(new QueryOption("$expand", $"SingleValueExtendedProperties($filter=Id eq 'Binary 0x0FFF')"));

            var childFolders = await childFoldersReq.GetAsync();
            var childFolderIds = childFolders.ToList().ToDictionary(f => f.Id, f => BitConverter.ToString(Convert.FromBase64String(f.SingleValueExtendedProperties?.FirstOrDefault()?.Value ?? "")).Replace("-", ""));

            foreach (var folder in childFolders)
            {
                if (folder.ChildFolderCount > 0)
                    childFolderIds.AddRange(await graphClient.GetChildFolderIds(folder.Id));
            }

            return childFolderIds;
        }

        public static async Task<MailFolder> GetMailFolder(this GraphServiceClient graphClient, string folderId, bool expandChildFolders = false)
        {
            var req = graphClient.Me.MailFolders[folderId].Request();

            if (expandChildFolders)
                req.Expand("childFolders");

            return await req.GetAsync();
        }

        public static async Task<List<MailFolder>> GetMailFolders(this GraphServiceClient graphClient)
        {
            var mailFolders = (await graphClient.Me.MailFolders.Request().Top(100).GetAsync())
                                .Where(f => MailFolders.Contains(f.DisplayName))
                                .OrderBy(f => MailFolders.IndexOf(f.DisplayName))
                                .ToList();

            foreach (var mailFolder in mailFolders)
            {
                if (mailFolder.ChildFolderCount > 0)
                    mailFolder.ChildFolders = await graphClient.GetChildFolders(mailFolder.Id); ;
            }

            return mailFolders;
        }

        public static async Task<IMailFolderChildFoldersCollectionPage> GetChildFolders(this GraphServiceClient graphClient, string folderId)
        {
            var childFolders = await graphClient.Me.MailFolders[folderId].ChildFolders.Request().Top(100).GetAsync();

            foreach (var folder in childFolders)
            {
                if (folder.ChildFolderCount > 0)
                    folder.ChildFolders = await graphClient.GetChildFolders(folder.Id);
            }

            return childFolders;
        }

        public static async Task<MailFolder> GetParentFolder(this GraphServiceClient graphClient, string folderId)
        {
            var mailFolders = await graphClient.GetMailFolders();

            foreach (var folder in mailFolders)
            {
                if (folder.Id == folderId || (folder.ChildFolderCount > 0 && folder.ChildFolders.ContainsFolder(folderId)))
                    return folder;
            }

            return new MailFolder();
        }

        public static bool ContainsFolder(this IMailFolderChildFoldersCollectionPage childFolders, string folderId)
        {
            bool found = false;

            foreach (var folder in childFolders)
            {
                found = (folder.Id == folderId);

                if (!found && folder.ChildFolderCount > 0)
                    found = folder.ChildFolders.ContainsFolder(folderId);

                if (found)
                    return true;
            }

            return false;
        }

        public static List<MailFolder> Flatten(this List<MailFolder> mailFolders, bool indentName = false)
        {
            var flatFolders = new List<MailFolder>();

            foreach (var folder in mailFolders)
            {
                flatFolders.AddRange(FlattenChildFolders(folder, indentName));
            }

            return flatFolders;
        }

        public static List<MailFolder> FlattenChildFolders(this MailFolder folder, bool indentName = false)
        {
            var flatFolders = new List<MailFolder>() { folder };

            if (folder.ChildFolders != null)
            {
                var pad = indentName ? folder.DisplayName.IndexOf(folder.DisplayName.TrimStart()) + 1 : 0;

                foreach (var childFolder in folder.ChildFolders)
                {
                    if (indentName)
                        childFolder.DisplayName = childFolder.DisplayName.PadLeft(childFolder.DisplayName.Length + pad);

                    flatFolders.AddRange(FlattenChildFolders(childFolder, indentName));
                }
            }

            return flatFolders;
        }

        //https://docs.microsoft.com/en-us/graph/api/message-reply?view=graph-rest-1.0&tabs=http
        //https://docs.microsoft.com/en-us/graph/api/message-replyall?view=graph-rest-1.0&tabs=csharp
        public static async Task Reply(this GraphServiceClient graphClient, string messageId, Message message, string comment)
        {
            await graphClient.Me.Messages[messageId]
                .Reply(message, comment)
                .Request()
                .PostAsync();
        }

        //https://docs.microsoft.com/en-us/graph/api/message-forward?view=graph-rest-1.0&tabs=http
        public static async Task Forward(this GraphServiceClient graphClient, string messageId, IEnumerable<Recipient> toRecipients, Message message, string comment)
        {
            await graphClient.Me.Messages[messageId]
                .Forward(toRecipients, message, comment)
                .Request()
                .PostAsync();
        }

        public static async Task<Stream> Download(this GraphServiceClient graphClient, string messageId)
        {
            return await graphClient.Me.Messages[messageId].Content.Request().GetAsync();
        }

        public static async Task<Message> GetMessage(this GraphServiceClient graphClient, string messageId)
        {
            return await graphClient.Me.Messages[messageId].Request().Expand("attachments").GetAsync();
        }

        public static async Task<(List<Message> Page, int Count)> GetMessages(this GraphServiceClient graphClient, DataSourceRequest request, List<QueryFilterViewModel> mainSearchFilters, string? select = null)
        {
            var req = graphClient.Me.MailFolders[mainSearchFilters.GetMailFolderId()].Messages.Request();
            req.QueryOptions.Add(new QueryOption("$count", "true"));
            req.BuildQueryParameters(request, mainSearchFilters, select);

            var msgs = await req.GetAsync();
            var count = 0;

            msgs.AdditionalData.TryGetValue("@odata.count", out var msgsCount);

            if (msgsCount != null)
                count = int.Parse(msgsCount.ToString());

            return (Page: msgs.CurrentPage.ToList(), Count: count);
        }

        public static async Task<(List<MailListViewModel> Page, int Count)> GetMailListViewModel(this GraphServiceClient graphClient, DataSourceRequest request, List<QueryFilterViewModel> mainSearchFilters, string? select = null)
        {
            var msgs = await graphClient.GetMessages(request, mainSearchFilters);

            return (Page: msgs.Page.AsQueryable().ProjectTo<MailListViewModel>().ToList(), Count: msgs.Count);
        }

        public static string GetMailFolderId(this List<QueryFilterViewModel> mainSearchFilters)
        {
            var folderId = "Inbox";
            var folderProperty = mainSearchFilters.FirstOrDefault(f => f.Property == "Folder");

            if (folderProperty != null)
            {
                folderId = folderProperty.Value;
                mainSearchFilters.Remove(folderProperty);
            }

            return folderId;
        }

        public static string GetMailboxName(this List<QueryFilterViewModel> mainSearchFilters)
        {
            var mailbox = "";
            var mailboxProperty = mainSearchFilters.FirstOrDefault(f => f.Property == "Mailbox");

            if (mailboxProperty != null)
            {
                mailbox = mailboxProperty.Value;
                mainSearchFilters.Remove(mailboxProperty);
            }

            return mailbox;
        }

        //https://docs.microsoft.com/en-us/graph/query-parameters
        public static void BuildQueryParameters(this IMailFolderMessagesCollectionRequest messages, DataSourceRequest request,
                                                List<QueryFilterViewModel> mainSearchFilters,
                                                string? select = "id,from,toRecipients,subject,bodyPreview,receivedDateTime,hasAttachments,isRead,internetMessageId")
        {
            var sortFilterProperty = "";

            if (request.Sorts.Any())
            {
                var sort = request.Sorts.FirstOrDefault();
                var sortProperty = sort.Member;
                var sortDirection = sort.SortDirection == Kendo.Mvc.ListSortDirection.Ascending ? "asc" : "desc";

                sortFilterProperty = sort.Member;

                switch (sort.Member)
                {
                    case "Name":
                        sortProperty = "from/emailAddress/name";
                        sortFilterProperty = "FromNameOrAddress";
                        break;

                    case "Address":
                        sortProperty = "from/emailAddress/address";
                        sortFilterProperty = "FromNameOrAddress";
                        break;

                    case "ReceivedDateTime":
                        sortFilterProperty = "ReceivedDateTimeTo";
                        break;
                }

                messages.OrderBy($"{sortProperty} {sortDirection}");
            }

            if (mainSearchFilters.Any())
            {
                var filterString = "";

                //$orderby must be in $filter
                //https://devblogs.microsoft.com/microsoft365dev/update-to-filtering-and-sorting-rest-api/
                if (!string.IsNullOrEmpty(sortFilterProperty))
                {
                    var sortFilter = mainSearchFilters.FirstOrDefault(f => f.Property == sortFilterProperty);
                    var sortFilterValue = "";

                    if (sortFilter != null)
                    {
                        sortFilterValue = sortFilter.Value;
                        mainSearchFilters.Remove(sortFilter);
                    }
                    filterString = BuildMessageFilter(sortFilterProperty, sortFilterValue);
                }

                foreach (var filter in mainSearchFilters)
                {
                    var msgFilter = BuildMessageFilter(filter.Property, filter.Value);
                    if (!string.IsNullOrEmpty(msgFilter))
                        filterString = (string.IsNullOrEmpty(filterString) ? "" : $"{filterString} and ") + msgFilter;
                }

                if (!string.IsNullOrEmpty(filterString))
                    messages.Filter(filterString);

                //$search
                //$skip is not supported with $search
                //var searchString = "";
                //foreach (var filter in mainSearchFilters)
                //{
                //    var msgFilter = BuildMessageSearch(filter.Property, filter.Value);
                //    if (!string.IsNullOrEmpty(msgFilter))
                //        searchString = (string.IsNullOrEmpty(searchString) ? "" : $"{searchString} AND ") + msgFilter;
                //}

                //if (!string.IsNullOrEmpty(searchString))
                //    messages.QueryOptions.Add(new QueryOption("$search", $"\"{searchString}\""));

            }

            //include attachments
            //messages.QueryOptions.Add(new QueryOption("$expand", $"attachments($select=name,size,contentType)"));

            messages.Select(select);
            messages.Skip((request.Page - 1) * request.PageSize);
            messages.Top(request.PageSize);
        }

        public static string BuildMessageFilter(string property, string value)
        {
            value = value.Replace("'", "''");

            switch (property)
            {
                case "FromNameOrAddress":
                    return $"(contains(from/emailAddress/name,'{value}') or contains(from/emailaddress/address,'{value}'))";

                //ToRecipients, CcRecipients, BccToRecipients are not filtereable
                //https://docs.microsoft.com/en-us/previous-versions/office/office-365-api/api/version-2.0/complex-types-for-mail-contacts-calendar#message
                //case "ToRecipients":
                //    return $"(toRecipients/any(r: contains(r/emailAddress/name,'{value}') or contains(r/emailaddress/address,'{value}'))";

                case "Subject":
                    return $"contains(subject,'{value}')";

                case "Body":
                    return $"contains(body/content,'{value}')";

                case "Keyword":
                    return $"(contains(subject,'{value}') or contains(body/content,'{value}'))";

                case "ReceivedDateTimeFrom":
                    return $"receivedDateTime ge {DateTime.Parse(value).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssK")}";

                case "ReceivedDateTimeTo":
                    return $"receivedDateTime le {(string.IsNullOrEmpty(value) ? DateTime.Now.AddDays(1) : DateTime.Parse(value)).AddDays(1).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssK")}";

                case "Attachments":
                    return $"hasAttachments eq true";

                case "Unread":
                    return $"isRead ne true";
            }
            return "";
        }

        //$search
        //$skip is not supported.
        //$count is not supported. A $search request returns up to 1000 results.
        //https://docs.microsoft.com/en-us/graph/search-query-parameter?tabs=http
        public static string BuildMessageSearch(string property, string value)
        {
            value = value.Replace("\"", "\\\"");
            var messageProperty = "";
            var searchOp = "";

            switch (property)
            {
                case "FromNameOrAddress":
                    messageProperty = "from";
                    searchOp = ":";
                    break;

                case "ToRecipients":
                    messageProperty = "to";
                    searchOp = ":";
                    break;

                case "CcRecipients":
                    messageProperty = "to";
                    searchOp = ":";
                    break;

                case "Subject":
                    messageProperty = "subject";
                    searchOp = ":";
                    break;

                case "ReceivedDateTimeFrom":
                    messageProperty = "received";
                    searchOp = ">=";
                    break;

                case "ReceivedDateTimeTo":
                    messageProperty = "received";
                    searchOp = "<=";
                    break;

                case "Attachments":
                    messageProperty = "hasAttachment";
                    searchOp = ":";
                    break;

                case "Keyword":
                    searchOp = "";
                    break;

                default:
                    value = "";
                    break;
            }
            return $"{messageProperty}{searchOp}{value}";
        }
    }
}
