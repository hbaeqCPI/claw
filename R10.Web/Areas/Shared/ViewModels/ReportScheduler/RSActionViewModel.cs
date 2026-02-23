using R10.Core.Entities.ReportScheduler;
using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Shared.ViewModels.ReportScheduler
{
    public class RSActionViewModel: RSAction
    {
        [Required]
        public int ParentId { get; set; }

        RSActionType? rSActionType { get; set; }

        public string? Status { get; set; }
    }
}
