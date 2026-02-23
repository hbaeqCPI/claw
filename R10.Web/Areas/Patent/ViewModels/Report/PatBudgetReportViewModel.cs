using R10.Web.Areas.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Patent
{
    public class PatBudgetReportViewModel : ReportBaseViewModel, IValidatableObject
    {
        public int SortOrder { get; set; }
        public bool PrintShowCriteria { get; set; }
        public bool PrintShowCriteriaOnFirstPage { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        IEnumerable<ValidationResult> IValidatableObject.Validate(ValidationContext validationContext)
        {
            List<ValidationResult> res = new List<ValidationResult>();
            if (ToDate != null && FromDate != null && ToDate - FromDate < TimeSpan.FromDays(0))
            {
                ValidationResult mss = new ValidationResult("The To Date should be later than the From Date.");
                res.Add(mss);

            }
            return res;
        }
        public string? CountryOp { get; set; }
        public string? CaseNumberFrom { get; set; }
        public string? CaseNumberTo { get; set; }
        public string? Area { get; set; }
        public string? CostTypesOp { get; set; }
        public string? CostTypes { get; set; }
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
        public string? Titles { get; set; }
        public string? Title { get; set; }
        public string? LabelReportTitle { get; set; }
    }
}
