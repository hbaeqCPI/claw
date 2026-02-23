using R10.Core.Entities.Trademark;
using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Trademark.ViewModels
{
    public class TmkTrademarkClassViewModel : TmkTrademarkClassDetail
    {
        [UIHint("TmkStandardGood")]
        public TmkStandardGoodListViewModel TmkStandardGood { get; set; }           // should be same name as base entity TmkStandardGood
    }
}
