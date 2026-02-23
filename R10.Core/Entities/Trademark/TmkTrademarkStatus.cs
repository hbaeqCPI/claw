using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.Trademark
{
    public class TmkTrademarkStatus : BaseEntity
    {
        public int TrademarkStatusId { get; set; }

        [Key]
        [Required]
        [StringLength(20)]
        [Display(Name = "Trademark Status")]
        public string? TrademarkStatus { get; set; }

        [StringLength(255)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Active?")]
        public bool ActiveSwitch { get; set; } = true;

        public bool CPITmkStatus { get; set; } = false;

        public List<TmkTrademark>? TmkTrademarks { get; set; }

    }
}
