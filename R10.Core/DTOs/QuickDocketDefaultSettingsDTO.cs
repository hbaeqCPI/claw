using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Core.DTOs
{
    public class QuickDocketDefaultSettingsDTO
    {
        public string? ActionDue { get; set; }

        public string? ActionType { get; set; }
        public string[]? ActionTypes { get; set; }

        public string? ActiveSwitch { get; set; } = "2";
        public string? OfficeAction { get; set; } = "2";

        public string? Agent { get; set; }

        public string? Attorney { get; set; }
        public string[]? Attorneys { get; set; }

        public int? BaseDateEndOffset { get; set; }

        public string? BaseDateEndUnit { get; set; }

        public string? BaseDateRange { get; set; } = "None";

        public int? BaseDateStartOffset { get; set; }

        public string? BaseDateStartOp { get; set; }

        public string? BaseDateStartUnit { get; set; }

        public string? BaseDateTimeFrame { get; set; }

        public string? CaseNumber { get; set; }
        
        public string? CaseType { get; set; }

        public string? Client { get; set; }
        public string[]? Clients { get; set; }

        public string? ClientRef { get; set; }

        public string? Country { get; set; }

        public string? CountryOp { get; set; } = "=";

        public int? DueDateEndOffset { get; set; } = 0;

        public string? DueDateEndUnit { get; set; } = "M";

        public string? DueDateRange { get; set; } = "Relative";

        public int? DueDateStartOffset { get; set; } = 0;

        public string? DueDateStartOp { get; set; } = "-";

        public string? DueDateStartUnit { get; set; } = "D";

        public string? DueDateTimeFrame { get; set; } = "W";

        public string? Indicator { get; set; }

        public string[]? Indicators { get; set; }

        public string? IndicatorOp { get; set; } = "=";

        public bool IsDefaultPage { get; set; } = false;

        public string? Owner { get; set; }

        public string? RespOffice { get; set; }

        public string? Responsible { get; set; }

        public string? StatusOp { get; set; } = "=";

        public string? Status { get; set; }
        
        public string? SystemTypes { get; set; }

        public string? Title { get; set; }

        public string? FilterAtty { get; set; } = "A1|A2|A3|AR";

        //dedocket instructions
        public DateTime? FromInstructionDate { get; set; }
        public DateTime? ToInstructionDate { get; set; }
        public string? DeDocketInstruction { get; set; }
        public string? DeDocketInstructedBy { get; set; }
        public bool? DeDocketInstructionOnly { get; set; }
        public bool? DeDocketUninstructedOnly { get; set; }
        public int? DeDocketInstrxDateEndOffset { get; set; }
        public string? DeDocketInstrxDateEndUnit { get; set; }
        public string? DeDocketInstrxDateRange { get; set; } = "None";
        public int? DeDocketInstrxDateStartOffset { get; set; }
        public string? DeDocketInstrxDateStartOp { get; set; }
        public string? DeDocketInstrxDateStartUnit { get; set; }
        public string? DeDocketInstrxDateTimeFrame { get; set; }

        public bool IncludeNP { get; set; } = false;
    }
}
