using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Entities.Shared
{
    public class ActionDueWebSvcDetail
    {

        [Required]
        [StringLength(25)]
        public string? CaseNumber { get; set; }

        [Required]
        [StringLength(5)]
        public string? Country { get; set; }

        [StringLength(8)]
        public string? SubCase { get; set; }

        [Required]
        [StringLength(60)]
        public string? ActionType { get; set; }

        [Required]
        public DateTime? BaseDate { get; set; }

        public DateTime? ResponseDate { get; set; }

        [StringLength(5)]
        public string? Attorney { get; set; }

        public string? Remarks { get; set; }
    }
}
