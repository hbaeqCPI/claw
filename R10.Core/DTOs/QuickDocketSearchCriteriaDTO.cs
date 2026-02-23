using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Core.DTOs
{
    public class QuickDocketSearchCriteriaDTO
    {        
        // NOTE: Order of the fields must match stored procedure
        public string? SystemTypes { get; set; }

        public string? ActiveSwitch { get; set; }
        public string? OfficeAction { get; set; } = "2";

        public string? RespOffice { get; set; }

        public DateTime? FromDueDate { get; set; } 

        public DateTime? ToDueDate { get; set; }
        
        public string? ActionType { get; set; }

        public DateTime? FromBaseDate { get; set; } 

        public DateTime? ToBaseDate { get; set; } 

        public string? ActionDue { get; set; }

        public string? Responsible { get; set; }

        public string? Attorney { get; set; }

        public string? IndicatorOp { get; set; }

        public string? Indicator { get; set; }

        public string? CaseNumber { get; set; }

        public string? CountryOp { get; set; }

        public string? Country { get; set; }        

        public string? CaseType { get; set; }

        public string? Client { get; set; }

        public string? ClientRef { get; set; }

        public string? Title { get; set; }

        public string? Owner { get; set; }

        public string? Agent { get; set; }
                
        public DateTime? FromInstrxDate { get; set; }
        public DateTime? ToInstrxDate { get; set; }

        //dedocket instructions
        public DateTime? FromInstructionDate { get; set; }
        public DateTime? ToInstructionDate { get; set; }
        public string? DeDocketInstruction { get; set; }
        public string? DeDocketInstructedBy { get; set; }
        public bool? DeDocketInstructionOnly { get; set; }
        public bool? DeDocketUninstructedOnly { get; set; }
        public bool? DeDocketInstrCompleted { get; set; }
        public bool? DeDocketUninstructedSwitch { get; set; }

        public string? StatusOp { get; set; }

        public string? Status { get; set; }

        public string? FilterAtty { get; set; } 

        public string? SortCol { get; set; }

        public string? SortOrder { get; set; }

        public int? MaximumRows { get; set; }

        public int? StartRowIndex { get; set; }

        public string? TargetData { get; set; } = "lst";

        public string? InBehalfOf { get; set; }
        public bool? Delegated { get; set; }
        public bool? TrackOne { get; set; }
        public bool? PODocketed { get; set; }

        public bool IncludeNP { get; set; } = false;
        public int SoftDocket { get; set; } = 0;
    }

    public class QuickDocketUpdateCriteriaDTO: QuickDocketSearchCriteriaDTO
    {
        public string? DateType { get; set; }
        public DateTime? SpecificDate { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? LastUpdate { get; set; }
        
        public string? NewDeDocketInstruction { get; set; }
        public string? NewDeDocketRemarks { get; set; }
        public bool EmptyInstructionOnly { get; set; }
        public string? UserId { get; set; }
        public List<string> RecIds { get; set; }

    }

    public class QuickDocketDeDocketBatchUpdateResultDTO
    {
        public int DedocketId { get; set; } = 0;
        public string? SystemType { get; set; }
        public int ActId { get; set; } = 0;
        public string? Instruction { get; set; }
        public int ParentId { get; set; } = 0;

    }

}
