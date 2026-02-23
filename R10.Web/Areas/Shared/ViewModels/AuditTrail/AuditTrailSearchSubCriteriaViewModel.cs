
namespace R10.Web.Areas.Shared.ViewModels
{
   
    public class AuditTrailSearchSubCriteriaViewModel
    {
        public string? SystemType { get; set; }
        public string? SystemName { get; set; }
        public string? ControllerName { get; set; }
        public string? AreaName { get; set; }
        public string? ValueMapper { get; set; }

        public string? LabelCaseNumber { get; set; }
        public bool EnableComboPaging { get; set; }
        public int ComboPagingSize { get; set; }

    }
}
