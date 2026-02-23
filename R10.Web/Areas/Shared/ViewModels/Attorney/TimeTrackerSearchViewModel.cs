
namespace R10.Web.Areas.Shared.ViewModels
{
    public class TimeTrackerSearchViewModel
    {
        public int SearchAttorneyId { get; set; }
        public string? SearchUserId { get; set; }
        public string? SearchSystemType { get; set; }
        public string? SearchCaseNumber { get; set; }
        public string? SearchCountry { get; set; }
        public string? SearchSubCase { get; set; }
        public string? SearchClientCode { get; set; }
        public string? SearchOutstandingOnly { get; set; }
        public DateTime? EntryDateFrom { get; set; }
        public DateTime? EntryDateTo { get; set; }
    }
}
