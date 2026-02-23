using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace R10.Core.DTOs
{
    [Keyless]
    public class GMGlobalUpdatePreviewDTO
    {
        public string? CaseNumber { get; set; }      
        public string? SubCase { get; set; }
        public string? MatterType { get; set; }
        public string? MatterTitle { get; set; }
        public string? MatterStatus { get; set; }
        public bool ActiveSwitch { get; set; }
        public string? Client { get; set; }        
        public string? Agent { get; set; }
        public string? ReferenceNumber { get; set; }
        //public string? Attorney { get; set; }

        public DateTime? EffectiveOpenDate { get; set; }        
        public DateTime? TerminationEndDate { get; set; }
        
        public string? RespOffice { get; set; }

        public int? KeyId { get; set; }
        public string? ActionType { get; set; }
        public string? Responsible { get; set; }
        public DateTime? BaseDate { get; set; }

        public string? InvoiceNumber { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public DateTime? PayDate { get; set; }

        public string? ErrorConflict { get; set; }
        [NotMapped]
        public bool Selected { get; set; } = true;
    }
}
