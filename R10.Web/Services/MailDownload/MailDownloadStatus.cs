namespace R10.Web.Services.MailDownload
{
    public class MailDownloadStatus
    {
        public MailDownloadStatusType Status { get; set; }
        public DateTime? ReceivedDateTimeFrom { get; set; }
        public DateTime? ReceivedDateTimeTo { get; set; }
        public int RuleId { get; set; }
        public int LogId { get; set; }
    }

    public enum MailDownloadStatusType
    {
        Completed,
        Running,
        Expired
    }
}
