
namespace R10.Web.Areas.Shared.ViewModels
{
    public class LetterGenParamViewModel
    {
        public int LetId { get; set; }
        public bool IsLog { get; set; }
        public string? SystemType { get; set; }
        public IEnumerable<LetterEntityContactViewModel>? SelectedContacts;
        public string? ScreenSource { get; set; }
        public string? LetterScreenCode { get; set; } = "";
        public int RecordId { get; set; } = 0;
        public string? PreviewSelection { get; set; } = "";
    }
}
