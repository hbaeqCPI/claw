using R10.Web.Areas.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Patent.ViewModels.Report
{
    public class PLUSReportViewModel : ReportBaseViewModel, IValidatableObject
    {
        public int SortOrder { get; set; }
        public int ReportType { get; set; }
        public string? CaseNumber { get; set; }
        public string? Client { get; set; }
        public string? ClientName { get; set; }
        public string? Attorney { get; set; }
        public string? AttorneyName { get; set; }
        public string? Examiner { get; set; }
        public string? Examiners { get; set; }
        public string? GroupArt { get; set; }
        public string? Confirmation { get; set; }
        public string? Class { get; set; }
        public string? RespOffice { get; set; }
        public string? RespOffices { get; set; }
        public string? CaseType { get; set; }
        public string? ActiveSwitch { get; set; }
        public string? Correspondence { get; set; }
        public DateTime? TranDateFrom { get; set; }
        public DateTime? TranDateTo { get; set; }
        public string? TranDesc { get; set; }
        public string? TranDescs { get; set; }
        public DateTime? MailDateFrom { get; set; }
        public DateTime? MailDateTo { get; set; }
        public string? DocDescription { get; set; }
        public bool PrintShowCriteria { get; set; }
        public bool PrintShowCriteriaOnFirstPage { get; set; }
        IEnumerable<ValidationResult> IValidatableObject.Validate(ValidationContext validationContext)
        {
            List<ValidationResult> res = new List<ValidationResult>();
            if (TranDateTo != null && TranDateFrom != null && TranDateTo - TranDateFrom < TimeSpan.FromDays(0))
            {
                ValidationResult mss = new ValidationResult("The Transaction To Date should be later than the From Date.");
                res.Add(mss);

            }
            if (MailDateTo != null && MailDateFrom != null && MailDateTo - MailDateFrom < TimeSpan.FromDays(0))
            {
                ValidationResult mss = new ValidationResult("The Mail Room To Date should be later than the From Date.");
                res.Add(mss);

            }
            return res;
        }
    }
}
