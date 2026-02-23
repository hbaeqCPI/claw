namespace R10.Web.Areas.Shared.ViewModels
{
    public class SharedDueDateDelegationReportViewModel : ReportBaseViewModel
    {
        public bool PrintActionDueRemarks { get; set; }
        public bool PrintDueDateRemarks { get; set; }
        public bool PrintRemarks { get; set; }
        public string? PrintGoods { get; set; }
        public bool PrintImage { get; set; }
        public bool PrintImageDetail { get; set; }
        public bool PrintInventors { get; set; }
        public bool DeDocketInstructionOnly { get; set; }
        public string? PrintSystems { get; set; }
        public string? ActiveSwitch { get; set; }
        public string? UserID { get; set; }
        public int StartDateAdjustment { get; set; }
    }
}