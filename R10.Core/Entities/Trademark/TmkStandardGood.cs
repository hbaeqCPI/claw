using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Trademark
{
    public class TmkStandardGood : TmkStandardGoodDetail
    {
    }

    public class TmkStandardGoodDetail : BaseEntity
    {
        [Key]
        public int ClassId { get; set; }

        [StringLength(3)]
        [Required]
        [Display(Name ="Class")]
        public string? Class { get; set; }

        [StringLength(40)]
        [Required]
        [Display(Name = "Class Type")]
        public string? ClassType { get; set; }

        [Display(Name = "Standard Goods")]
        public string? StandardGoods { get; set; }
    }

}
