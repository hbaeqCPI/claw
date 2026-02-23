
namespace R10.Web.Areas.Shared.ViewModels
{
    public class FFAgentResponsibilityCoverLetter : EmailContent
    {
        public string? SenderName { get; set; }
        public string? SenderEmail { get; set; }
        public string? AgentCode { get; set; }
        public string? AgentName { get; set; }
        public string? ContactName { get; set; }
        public string? ContactGreeting { get; set; }
        public string? RecipientList { get; set; }
    }
}
