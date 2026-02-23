
namespace R10.Web.Areas.Shared.ViewModels
{
    public class ProductPrintViewModel : ReportBaseViewModel
    {
        public string? IDs { get; set; }

        public bool PrintRelatedProducts { get; set; }

        public bool PrintSales { get; set; }
        public bool PrintRelatedPatents { get; set; }
        public bool PrintRelatedTrademarks { get; set; }
        public bool PrintRelatedMatters { get; set; }
        public bool PrintChart { get; set; }
    }
}
