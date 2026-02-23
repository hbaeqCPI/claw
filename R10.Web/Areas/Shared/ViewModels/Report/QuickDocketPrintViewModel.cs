
namespace R10.Web.Areas.Shared.ViewModels
{
    public class QuickDocketPrintViewModel : ReportBaseViewModel
    {
        public string? SystemTypes { get; set; }
        public string? ActiveSwitch { get; set; }
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
        public DateTime? FromInstructionDate { get; set; }
        public DateTime? ToInstructionDate { get; set; }
        public string? DeDocketInstruction { get; set; }
        public string? DeDocketInstructedBy { get; set; }
        public bool? DeDocketInstructionOnly { get; set; }
        public bool? DeDocketUninstructedOnly { get; set; }
        public bool? DeDocketInstrCompleted { get; set; }
        public string? StatusOp { get; set; }
        public string? Status { get; set; }
        public string? FilterAtty { get; set; }
        public string? SortCol { get; set; }
        public string? SortOrder { get; set; }
        public int? MaximumRows { get; set; }
        public int? StartRowIndex { get; set; }
        public string? TargetData { get; set; }
        public string? InBehalfOf { get; set; }
        public bool? Delegated { get; set; }
        public bool? TrackOne { get; set; }
        public bool? PODocketed { get; set; }
        public int RowCount { get; set; } 
        public int? SoftDocket { get; set; } 
    }
}
