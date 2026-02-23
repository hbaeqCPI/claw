using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace R10.Core.DTOs
{
    [Keyless]
    public class TLSearchGoodsDTO
    {
        public int TLTmkId { get; set; }

        [Display(Name = "Intl. Class")]
        public string? Class { get; set; }

        [Display(Name = "Basis")]
        public string? Basis { get; set; }

        [Display(Name = "First Use Date")]
        public DateTime? FirstUseDate { get; set; }

        [Display(Name = "First Use In Commerce")]
        public DateTime? FirstUseInCommerceDate { get; set; }

        [Display(Name = "Goods")]
        public string? Goods { get; set; }

    }
}
