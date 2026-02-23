using System;
using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Trademark
{
    public class TmkTrademarkClass : TmkTrademarkClassDetail
    {
        public TmkTrademark? TmkTrademark { get; set; }

        public TmkStandardGood? TmkStandardGood { get; set; }
    }
    public class TmkTrademarkClassDetail : BaseEntity
    {
        [Key]
        public int TmkClassId { get; set; }

        [Required]
        public int TmkId { get; set; }

        [Required]
        public int ClassId { get; set; }

        public string? Goods { get; set; }

        public bool IsStandardGoods { get; set; }

        [Display(Name = "First Use Date")]
        public DateTime? FirstUseDate { get; set; }

        [Display(Name = "First Use in Commerce")]
        public DateTime? FirstUseInCommerce { get; set; }
    }
}
