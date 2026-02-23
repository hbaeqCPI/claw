using R10.Web.Areas.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Patent.ViewModels.Report
{
    public class PatRemunerationReportViewModel : ReportBaseViewModel, IValidatableObject
    {
        public int SortOrder { get; set; }
        public bool PrintShowCriteria { get; set; }
        public bool PrintShowCriteriaOnFirstPage { get; set; }
        public int PaidOption { get; set; }
        public int RemunerationTypeOption { get; set; }
        public bool PrintRemarks { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? Inventor { get; set; }
        public string? Inventors { get; set; }
        public int? FromYear { get; set; }
        public int? ToYear { get; set; }
        public string? CaseNumber { get; set; }
        public string? CaseNumbers { get; set; }
        public string? Title { get; set; }
        public string? Titles { get; set; }
        public string? Client { get; set; }
        public string? Clients { get; set; }
        public string? ClientName { get; set; }
        public string? ClientNames { get; set; }
        public string? RespOffice { get; set; }
        public string? RespOffices { get; set; }

        IEnumerable<ValidationResult> IValidatableObject.Validate(ValidationContext validationContext)
        {
            List<ValidationResult> res = new List<ValidationResult>();
            if (ToDate != null && FromDate != null && ToDate - FromDate < TimeSpan.FromDays(0))
            {
                ValidationResult mss = new ValidationResult("The To Date should be later than the From Date.");
                res.Add(mss);
            }
            if (ToYear != null && FromYear != null && ToYear - FromYear < 0)
            {
                ValidationResult mss = new ValidationResult("The To Year should be later than the From Year.");
                res.Add(mss);
            }
            return res;
        }
    }
}
