using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class DueDateListReportViewModel: ReportBaseViewModel, IValidatableObject
    {
        public int SortOrder { get; set; }
        public bool PrintActionDueRemarks { get; set; }
        public bool PrintDueDateRemarks { get; set; }
        public bool PrintRemarks { get; set; }
        public string? PrintGoods { get; set; }
        public bool PrintImage { get; set; }
        public bool PrintImageDetail { get; set; }
        public bool PrintPastReminders { get; set; }
        public bool PrintSoftDocket { get; set; }
        public bool PrintInventors { get; set; }
        public bool PrintCustomFields { get; set; }
        public bool DeDocketInstructionOnly { get; set; }
        public bool PrintShowCriteria { get; set; }
        public bool PrintShowCriteriaOnFirstPage { get; set; }
        public string? PrintSystems { get; set; }
        public int LayoutFormat { get; set; }
        //[Required(ErrorMessage = "Please Enter From Date")]
        public DateTime? FromDate { get; set; }
        //[Required(ErrorMessage = "Please Enter To Date")]
        public DateTime? ToDate { get; set; }
        IEnumerable<ValidationResult> IValidatableObject.Validate(ValidationContext validationContext)
        {
            List<ValidationResult> res = new List<ValidationResult>();
            if (FromDate == null)
            {
                ValidationResult mss = new ValidationResult("Please Enter From Date");
                res.Add(mss);
            }
            if (ToDate == null)
            {
                ValidationResult mss = new ValidationResult("Please Enter To Date");
                res.Add(mss);
            }
            if (ToDate != null && FromDate != null && ToDate - FromDate < TimeSpan.FromDays(0))
            {
                ValidationResult mss = new ValidationResult("The To Date should be later than the From Date.");
                res.Add(mss);
            }
            return res;
        }
        public string? IndicatorOp { get; set; }
        public string? ActionTypeOp { get; set; }
        public string? ActionDueOp { get; set; }
        public string? ClientOp { get; set; }
        public string? CountryOp { get; set; }
        public string? Area { get; set; }
        public string? FilterAtty { get; set; }
        public string? ActiveSwitch { get; set; }
        public string? OfficeAction { get; set; }
        public string? TrackOne { get; set; }
        public string? AutoDocketedActions { get; set; }
        public string? ApplicationStatusesOp { get; set; }
        public string? ApplicationStatuses { get; set; }
        public string? CaseTypesOp { get; set; }
        public string? CaseTypes { get; set; }
        public string? CaseNumberOp { get; set; }
        public string? CaseNumber { get; set; }
        public string? CaseNumbers { get; set; }
        public string? FamilyNumberOp { get; set; }
        public string? FamilyNumber { get; set; }
        public string? FamilyNumbers { get; set; }
        public string? ActionType { get; set; }
        public string? ActionTypes { get; set; }
        public string? ActionDue { get; set; }
        public string? ActionDues { get; set; }
        public string? DueIndicators { get; set; }
        public string? Client { get; set; }
        public string? Clients { get; set; }
        public string? ClientName { get; set; }
        public string? ClientNames { get; set; }
        public string? Countries { get; set; }
        public string? CountryName { get; set; }
        public string? CountryNames { get; set; }
        public string? Attorney { get; set; }
        public string? Attorneys { get; set; }
        public string? AttorneyName { get; set; }
        public string? AttorneyNames { get; set; }
        public string? Owner { get; set; }
        public string? Owners { get; set; }
        public string? OwnerName { get; set; }
        public string? OwnerNames { get; set; }
        public string? Agent { get; set; }
        public string? Agents { get; set; }
        public string? AgentName { get; set; }
        public string? AgentNames { get; set; }
        public string? RespOffice { get; set; }
        public string? RespOffices { get; set; }
        public string? ClassesOp { get; set; }
        public string? Classes { get; set; }
        public string? Class { get; set; }
        public string? InstructedBy { get; set; }
        public string? InstructedBys { get; set; }
        public string? Keyword { get; set; }
        public string? Keywords { get; set; }
        public string? Product { get; set; }
        public string? Products { get; set; }
        public bool DelegatedActionsOnly { get; set; }
        public string? DelegatedUserGroups { get; set; }
    }
}
