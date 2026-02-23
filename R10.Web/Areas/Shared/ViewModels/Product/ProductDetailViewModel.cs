using R10.Core.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class ProductDetailViewModel : ProductDetail
    { 
        [NotMapped]
        public int ActivePatents { get; set; }
        [NotMapped]
        public int ActiveTrademarks { get; set; }
        [NotMapped]
        public int ActiveGeneralMatters { get; set; }

    }


}
