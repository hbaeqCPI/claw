
namespace R10.Web.Areas.Shared.ViewModels
{
    public class DOCXGenParamViewModel
    {
        public int DOCXId { get; set; }
        public bool IsLog { get; set; }
        public string? SystemType { get; set; }
        public string? SystemName { get; set; }
        //public IEnumerable<DOCXEntityContactViewModel>? SelectedContacts;
        public string? ScreenSource { get; set; }
        public string? DOCXScreenCode { get; set; } = "";
        public string? DataKey { get; set; }
        public int RecordId { get; set; } = 0;
        public int PageNo { get; set; }
        public int PageCount { get; set; }
        public string? Signatory { get; set; }
        public string? SharePointRecKey { get; set; }
        public string? DocDesc { get; set; }
    }
}
