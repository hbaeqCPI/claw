using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.RMS
{
    public class RMSInstrxTypeActionDetail : BaseEntity
    {
        [Key]
        public int InstrxTypeActionDetailId { get; set; }

        [Required]
        public int InstrxTypeActionId { get; set; }

        [Required]
        [StringLength(5)]
        public string InstructionType { get; set; }

        public RMSInstrxTypeAction? RMSInstrxTypeAction { get; set; }
        public RMSInstrxType? RMSInstrxType { get; set; }
    }
}
