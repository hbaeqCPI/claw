using R10.Core.Entities.Patent;
using R10.Core.Helpers;
using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Shared.ViewModels.QuickDocket
{

    public class QuickDocketTradeSecretInvViewModel
    {
        public int ActId { get; set; }

        [TradeSecret]
        public string? InvTitle { get; set; }

        public InventionTradeSecret? TradeSecret { get; set; }

        public InventionTradeSecretRequest? TradeSecretRequest { get; set; }
    }

    public class QuickDocketTradeSecretAppViewModel
    {
        public int ActId { get; set; }

        public string? Country { get; set; }

        [TradeSecret]
        public string? AppTitle { get; set; }

        public CountryApplicationTradeSecret? TradeSecret { get; set; }

        public InventionTradeSecretRequest? TradeSecretRequest { get; set; }

    }
}
