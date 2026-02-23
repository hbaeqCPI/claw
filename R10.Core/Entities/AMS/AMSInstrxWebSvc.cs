using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Entities.AMS
{
    public class AMSInstrxWebSvc : AMSInstrxWebSvcDetail
    {
        [Key]
        public int EntityId { get; set; }

        public int LogId { get; set; }
    }

    public class AMSInstrxWebSvcDetail
    {
        [Required]
        [StringLength(25)]
        public string? CaseNumber { get; set; }
        
        [Required]
        [StringLength(5)]
        public string? Country { get; set; }
                
        [StringLength(8)]
        public string? SubCase { get; set; }

        [StringLength(10)]
        public string? PaymentType { get; set; } = "ANNUITY";

        [Required]
        [StringLength(10)]
        public string? AnnuityYear { get; set; }

        [Required]
        [StringLength(20)]
        public string? Instruction { get; set; }
    }
}
