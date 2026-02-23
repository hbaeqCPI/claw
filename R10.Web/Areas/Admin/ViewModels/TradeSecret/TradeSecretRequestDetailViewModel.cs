using R10.Core.Entities.Shared;

namespace R10.Web.Areas.Admin.ViewModels
{
    public class TradeSecretRequestDetailViewModel : TradeSecretRequest
    {
        public string? Email { get; set; }
        public string? DetailUrl { get;set; }
    }
}
