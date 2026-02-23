using R10.Core.Entities.ReportScheduler;
using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Shared.ViewModels.ReportScheduler
{
    public class RSCriteriaViewModel: RSCriteria
    {
        public int parentId { get; set; }
        [Display(Name = "Field")]
        public string? FieldAlias { get; set; }
    }
}
