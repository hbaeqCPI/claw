using System.ComponentModel.DataAnnotations;
using R10.Core.Entities.Patent;

namespace R10.Web.Areas.Patent.ViewModels
{
    public class InventionInventorRemunerationProductSaleInfoViewModel : PatIRProductSale
    {
        public bool MissingCurrencyType { get; set; }
        public bool MissingExchangeRate { get; set; }
    }
}
