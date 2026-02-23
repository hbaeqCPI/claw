using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace R10.Core.Entities.AMS
{
    public class AMSInstrxChangeLog
    {
        [Key]
        public int LogID { get; set; }
        public int DueID { get; set; }

        [StringLength(20)]
        [Display(Name = "Instruction")]
        public string ClientInstruction { get; set; }
        [StringLength(5)]
        [Display(Name = "Instruction")]
        public string ClientInstructionType { get; set; }
        [Display(Name = "Instruction Date")]
        public DateTime ClientInstructionDate { get; set; }
        [StringLength(1)]
        public string ClientInstructionSource { get; set; }

        [StringLength(140)]
        [Display(Name = "Reason For Change")]
        public string ReasonForChange { get; set; }
        [Display(Name = "Date Changed")]
        public DateTime DateChanged { get; set; }

        [StringLength(20)]
        [Display(Name = "Changed By")]
        public string CreatedBy { get; set; }

        [Column(TypeName = "timestamp")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [MaxLength(8)]
        [ConcurrencyCheck]
        public byte[] tStamp { get; set; }

        public AMSDue AMSDue { get; set; }
        public AMSInstrxCPiLogDetail AMSInstrxCPiLogDetail { get; set; }
    }
}
