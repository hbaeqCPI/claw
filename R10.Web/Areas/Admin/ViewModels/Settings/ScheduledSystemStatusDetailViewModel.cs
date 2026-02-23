using R10.Core.Entities;
using R10.Core.Identity;
using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Admin.ViewModels
{
    public class ScheduledSystemStatusDetailViewModel : ScheduledTask
    {
        [Display(Name = "System Status")]
        public SystemStatusType? StatusType { get; set; }

        public string? Message { get; set; }

        [Display(Name = "Show Notification")]
        public bool? ShowNotification { get; set; }

        [Display(Name = "Sign out users")]
        public bool? UpdateSecurityStamp { get; set; }
    }
}
