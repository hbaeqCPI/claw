using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Trademark
{
    public class TmkTrademarkClassWebSvc : TmkTrademarkClassWebSvcDetail
    {
        [Key]
        public int EntityId { get; set; }

        public int LogId { get; set; }

        [Required]
        public int TmkId { get; set; }
    }

    public class TmkTrademarkClassWebSvcDetail
    {

        [Required]
        public int ClassId { get; set; }

        public string? Goods { get; set; }

        public bool? IsStandardGoods { get; set; }

        [Display(Name = "First Use Date")]
        public DateTime? FirstUseDate { get; set; }

        [Display(Name = "First Use in Commerce")]
        public DateTime? FirstUseInCommerce { get; set; }
    }
}
