using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.AMS
{
    public class AMSInstrxDecisionMgt : BaseEntity
    {
        [Key]
        public int DecisionMgtID { get; set; }

        public int DueID { get; set; }

        public int ContactID { get; set; }

        [StringLength(5)]
        [Display(Name = "Instruction")]
        public string? ClientInstructionType { get; set; }

        [Display(Name = "Instruction Date")]
        public DateTime? ClientInstructionDate { get; set; }

        [StringLength(1000)]
        [Display(Name = "Remarks")]
        public string? ClientInstrxRemarks { get; set; }

        public AMSDue? AMSDue { get; set; }
        public AMSInstrxType? AMSInstrxType { get; set; }
    }
}
