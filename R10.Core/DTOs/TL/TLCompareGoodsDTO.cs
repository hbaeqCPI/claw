using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace R10.Core.DTOs
{
    [Keyless]
    public class TLCompareGoodsDTO
    {
        [Display(Name = "Your Class")]
        public string? TMSClass { get; set; }

        [Display(Name = "Your Goods")]
        public string? TMSGoods { get; set; }

        [Display(Name = "PTO Class")]
        public string? TLClass { get; set; }

        [Display(Name = "PTO Goods")]
        public string? TLGoods { get; set; }
    }

    
}
