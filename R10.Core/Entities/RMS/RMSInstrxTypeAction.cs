using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.RMS
{
    public class RMSInstrxTypeAction : BaseEntity
    {
        [Key]
        public int InstrxTypeActionId { get; set; }

        [Required, StringLength(60), Display(Name = "Action Type")]
        public string ActionType { get; set; }

        public List<RMSInstrxTypeActionDetail>? RMSInstrxTypeActionDetail { get; set; }
    }
}
