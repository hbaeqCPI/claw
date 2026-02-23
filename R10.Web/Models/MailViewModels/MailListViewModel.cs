namespace R10.Web.Models.MailViewModels
{
    public class MailListViewModel
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Address { get; set; }
        public string? ToRecipients { get; set; }
        public string? CcRecipients { get; set; }
        public string? Subject { get; set; }
        public string? BodyPreview { get; set; }
        public DateTime ReceivedDateTime { get; set; }
        public bool HasAttachments { get; set; }
        public bool IsRead { get; set; }
        public string? DownloadFileName { get; set; } //filename for drag and drop
        public string? InternetMessageId { get; set; }
        public bool IsDownloaded { get; set; }
    }
}
