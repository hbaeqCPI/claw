using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.AMS
{
    public class AMSStatusChangeLog : BaseEntity
    {
        [Key]
        public int LogID { get; set; }
        public int DueID { get; set; }
        public int AnnID { get; set; }
        public DateTime? ClientInstructionSentToCPI { get; set; }
        [StringLength(5)]
        public string? TriggerInstructionType { get; set; }
        [StringLength(15)]
        public string? OldStatus { get; set; }
        [StringLength(15)]
        public string? NewStatus { get; set; }
        public bool ProcessFlag { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string? Remarks { get; set; }

        public AMSDue? AMSDue { get; set; }
        public AMSMain? AMSMain { get; set; }
        public AMSInstrxType? TriggerInstrxType { get; set; }
    }
}
