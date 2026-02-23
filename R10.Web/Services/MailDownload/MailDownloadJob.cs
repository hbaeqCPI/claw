using Microsoft.Graph;
using R10.Core.Entities.MailDownload;

namespace R10.Web.Services.MailDownload
{
    public class MailDownloadJob
    {
        public string MailboxName { get; set; }
        public List<Message> Messages { get; set; }
        public int ActionId { get; set; }
        public MailDownloadActionType ActionType { get; set; }
        public int RuleId { get; set; }
        public bool DownloadAttachments { get; set; }

        /// <summary>
        /// If multiple rules apply to a single message,
        /// stop processing succeeding rules if document link is found
        /// </summary>
        public bool StopProcessing { get; set; }

        public string? DownloadFolderId { get; set; }
        public bool DoNotMove { get; set; }

        public List<string>? Responsibles { get; set; }
    }
}
