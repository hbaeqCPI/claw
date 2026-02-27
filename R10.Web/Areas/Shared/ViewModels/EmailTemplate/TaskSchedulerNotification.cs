namespace R10.Web.Areas.Shared.ViewModels
{
    public class TaskSchedulerNotification : EmailContent
    {
        public string? TaskName { get; set; }
        public string? Message { get; set; }
        public string? CallToAction { get; set; }
        public string? CallToActionUrl { get; set; }
    }
}
