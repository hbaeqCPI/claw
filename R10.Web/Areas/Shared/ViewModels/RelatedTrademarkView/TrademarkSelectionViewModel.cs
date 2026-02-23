using R10.Core.Entities.Trademark;
using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class TrademarkSelectionViewModel : TmkTrademarkDetail
    {
        public List<TmkTrademarkClass>? TrademarkClasses { get; set; }
        [Display(Name = "Class")]
        public string? Classes { get; set; } = "";
    }
}
