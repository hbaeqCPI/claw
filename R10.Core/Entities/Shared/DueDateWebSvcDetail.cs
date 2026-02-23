using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Entities.Shared
{
    public class DueDateWebSvcDetail
    {
        [Required]
        [StringLength(60)]
        public string? ActionDue { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        [Required]
        [StringLength(20)]
        public string? Indicator { get; set; }

        public DateTime? DateTaken { get; set; }

        [StringLength(5)]
        public string? Attorney { get; set; }
    }
}
