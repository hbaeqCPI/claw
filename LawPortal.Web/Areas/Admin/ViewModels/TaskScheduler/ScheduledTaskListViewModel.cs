using LawPortal.Core.Entities;

namespace LawPortal.Web.Areas.Admin.ViewModels
{
    public class ScheduledTaskListViewModel
    {
        public int TaskId { get; set; }

        public string? Name { get; set; }
                
        public string? Description { get; set; }

        public bool? IsEnabled { get; set; }
                
        public string? Status { get; set; }

        public DateTime? NextRunTime { get; set; }

        public DateTime? LastRunTime { get; set; }
    }
}
