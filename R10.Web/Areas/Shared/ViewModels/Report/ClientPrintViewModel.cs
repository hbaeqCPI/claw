
namespace R10.Web.Areas.Shared.ViewModels
{
    public class ClientPrintViewModel : ReportBaseViewModel
    {
        public string? IDs { get; set; }

        public bool PrintContactPerson { get; set; }

        public bool PrintDesCountries { get; set; }

        public bool PrintReviewers { get; set; }
    }
}
