using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.RMS
{
    public class RMSInstrxType : BaseEntity
    {
        public int InstructionId { get; set; }

        [Key]
        [Required]
        [StringLength(5)]
        public string InstructionType { get; set; }

        [StringLength(100)]
        public string? Description { get; set; }

        [StringLength(20)]
        public string? ClientDescription { get; set; }

        [StringLength(20)]
        public string? AgentDescription { get; set; }

        [StringLength(20)]
        public string? ClientTrademarkStatus { get; set; }

        public Int16 OrderOfDisplay { get; set; }

        public bool Active { get; set; }

        public bool InUse { get; set; }

        public bool Remind { get; set; }

        public bool CloseAction { get; set; }

        public bool HideToClient { get; set; }

        public bool SendToAgent { get; set; }

        public bool GetCountries { get; set; }

        public List<RMSDue>? RMSDues { get; set; }

        public List<RMSInstrxTypeActionDetail>? RMSInstrxTypeActionDetail { get; set; }
        public List<RMSActionCloseLogDue>? SentInstructionTypes { get; set; }
    }

    //user editable settings
    public enum InstructionTypeSetting
    {
        InUse,
        HideToClient,
        CloseAction,
        SendToAgent
    }
}
