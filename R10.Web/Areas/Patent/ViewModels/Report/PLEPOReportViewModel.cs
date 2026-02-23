using R10.Web.Areas.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Patent.ViewModels.Report
{
    public class PLEPOReportViewModel : ReportBaseViewModel, IValidatableObject
    {
        public int ReportType { get; set; }
        public string? CaseNumber { get; set; }
        public string? SubCase { get; set; }
        public string? Client { get; set; }
        public string? ClientName { get; set; }
        public string? Attorney { get; set; }
        public string? AttorneyName { get; set; }
        public string? CaseTypes { get; set; }
        public string? Applicant { get; set; }
        public string? ApplAddress { get; set; }
        public string? ApplCountry { get; set; }
        public string? Inventor { get; set; }
        public string? InvAddress { get; set; }
        public string? InvCountry { get; set; }
        public string? IPClass { get; set; }
        public string? ExamAction { get; set; }
        public DateTime? ExamDateFrom { get; set; }
        public DateTime? ExamDateTo { get; set; }
        public string? RepName { get; set; }
        public string? RepCountry { get; set; }
        public DateTime? PriorityDateFrom { get; set; }
        public DateTime? PriorityDateTo { get; set; }
        public string? PrioCountry { get; set; }
        public string? RespOffice { get; set; }
        public string? RespOffices { get; set; }
        public string? ActiveSwitch { get; set; }
        public bool PrintShowCriteria { get; set; }
        public bool PrintShowCriteriaOnFirstPage { get; set; }
        IEnumerable<ValidationResult> IValidatableObject.Validate(ValidationContext validationContext)
        {
            List<ValidationResult> res = new List<ValidationResult>();
            if (ExamDateTo != null && ExamDateFrom != null && ExamDateTo - ExamDateFrom < TimeSpan.FromDays(0))
            {
                ValidationResult mss = new ValidationResult("The To Date should be later than the From Date.");
                res.Add(mss);

            }
            if (PriorityDateTo != null && PriorityDateFrom != null && PriorityDateTo - PriorityDateFrom < TimeSpan.FromDays(0))
            {
                ValidationResult mss = new ValidationResult("The Priority To Date should be later than the From Date.");
                res.Add(mss);

            }
            return res;
        }
    }
}
