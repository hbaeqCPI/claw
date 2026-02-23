using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.ForeignFiling
{
    public class FFInstrxTypeActionDetail : BaseEntity
    {
        [Key]
        public int InstrxTypeActionDetailId { get; set; }

        [Required]
        public int InstrxTypeActionId { get; set; }

        [Required]
        [StringLength(5)]
        public string InstructionType { get; set; }

        public FFInstrxTypeAction FFInstrxTypeAction { get; set; }
        public FFInstrxType FFInstrxType { get; set; }
    }
}
