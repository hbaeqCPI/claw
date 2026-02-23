using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class ProductIndexReportViewModel: ReportBaseViewModel
    {
        public bool PrintOtherProducts { get; set; }
        public bool PrintShowCriteria { get; set; }
        public bool PrintShowCriteriaOnFirstPage { get; set; }
        public bool PrintPatent{ get; set; }
        public bool PrintTrademark{ get; set; }
        public bool PrintGenMatter{ get; set; }
        public string? RespOffice { get; set; }
        public string? RespOffices { get; set; }
        public string? Title { get; set; }
        public string? Titles { get; set; }
        public string? Client { get; set; }
        public string? Clients { get; set; }
        public string? ClientName { get; set; }
        public string? ClientNames { get; set; }
        public string? Attorney { get; set; }
        public string? Attorneys { get; set; }
        public string? AttorneyName { get; set; }
        public string? AttorneyNames { get; set; }
        public string? Product { get; set; }
        public string? Products { get; set; }
        public string? ProductGroup { get; set; }
        public string? ProductGroups { get; set; }
        public string? ProductCategory { get; set; }
        public string? ProductCategories { get; set; }
        public string? Brand { get; set; }
        public string? Brands { get; set; }
    }
}
