namespace R10.Web.Areas.Patent.ViewModels
{
    public class InventionCountryApplicationSearchViewModel
    {
        public int CountryApplicationInvId { get; set; }
        public string? CountryApplicationCaseNumber { get; set; }
        public string? CountryApplicationCountry { get; set; }
        public string? CountryApplicationSubCase { get; set; }
        public string? CountryApplicationCaseType { get; set; }
        public string? CountryApplicationStatus { get; set; }
        public string? CountryApplicationAppNumber { get; set; }
        public DateTime? FilDateFrom { get; set; }
        public DateTime? FilDateTo { get; set; }
        public string? CountryApplicationPatNumber { get; set; }
        public DateTime? IssDateFrom { get; set; }
        public DateTime? IssDateTo { get; set; }
    }
}
