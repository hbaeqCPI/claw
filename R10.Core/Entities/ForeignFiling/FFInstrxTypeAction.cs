using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.ForeignFiling
{
    public class FFInstrxTypeAction : BaseEntity
    {
        [Key]
        public int InstrxTypeActionId { get; set; }

        [Required, StringLength(60), Display(Name = "Action Type")]
        public string ActionType { get; set; }

        [StringLength(60), Display(Name = "Foreign Filing Action Due")]
        public string? ActionDue { get; set; }

        [StringLength(5)]
        public string? Country { get; set; }

        public List<FFInstrxTypeActionDetail> FFInstrxTypeActionDetail { get; set; }
        public List<PatActionDue> PatActionDues { get; set; }
    }
}
