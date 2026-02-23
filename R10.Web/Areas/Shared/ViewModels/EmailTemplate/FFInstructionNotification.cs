
namespace R10.Web.Areas.Shared.ViewModels
{
    public class FFInstructionNotification : EmailContent
    {
        public string? SenderName { get; set; }
        public string? SenderEmail { get; set; }
        public string? ClientCode { get; set; }
        public string? CaseNumber { get; set; }
        public string? Country { get; set; }
        public string? Title { get; set; }
        public string? ActionDue { get; set; }
        public DateTime? DueDate { get; set; }
        public string? ClientInstruction { get; set; }
        public string? CallToAction { get; set; }
        public string? CallToActionUrl { get; set; }
    }
}
