using R10.Core.Entities;
using R10.Core.Identity;
using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Admin.ViewModels
{
    public class ScheduledSystemStatusListViewModel : ScheduledTaskListViewModel
    {
        public string? RequestContent { get; set; }

        [Display(Name = "System Status")]
        public string? StatusType { get; set; }
        
        public string? Message { get; set; }

        [Display(Name = "Show Notification")]
        public bool? ShowNotification { get; set; }

        [Display(Name = "Sign out users")]
        public bool? UpdateSecurityStamp { get; set; }
    }
}
