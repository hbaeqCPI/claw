using R10.Web.Areas.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Patent.ViewModels.Report
{
    public class PatScoreReportViewModel : ReportBaseViewModel, IValidatableObject
    {
        public int SortOrder { get; set; }
        public bool PrintInventors { get; set; }
        public bool PrintRemarks { get; set; }
        public bool PrintRatingRemarks { get; set; }
        public bool PrintAbstract { get; set; }
        public bool PrintRelatedMatter { get; set; }
        public bool PrintProducts { get; set; }
        public bool PrintSubjectMatters { get; set; }
        public bool PrintKeywords { get; set; }
        public bool PrintShowCriteria { get; set; }
        public bool PrintShowCriteriaOnFirstPage { get; set; }
        public int DateType { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public float? FromScore { get; set; }
        public float? ToScore { get; set; }
        public string? CaseNumber { get; set; }
        public string? CaseNumbers { get; set; }
        public string? Area { get; set; }
        public string? RespOffice { get; set; }
        public string? RespOffices { get; set; }
        public string? CaseTypesOp { get; set; }
        public string? CaseTypes { get; set; }
        public string? CountriesOp { get; set; }
        public string? Countries { get; set; }
        public string? CountryName { get; set; }
        public string? CountryNames { get; set; }
        public string? ActiveSwitch { get; set; }
        public string? ApplicationStatusesOp { get; set; }
        public string? ApplicationStatuses { get; set; }
        public string? Product { get; set; }
        public string? Products { get; set; }
        public string? SubjectMatter { get; set; }
        public string? SubjectMatters { get; set; }
        public string? Keyword { get; set; }
        public string? Keywords { get; set; }
        public string? Client { get; set; }
        public string? Clients { get; set; }
        public string? ClientName { get; set; }
        public string? ClientNames { get; set; }
        public string? Inventor { get; set; }
        public string? Inventors { get; set; }
        public string? Owner { get; set; }
        public string? Owners { get; set; }
        public string? OwnerName { get; set; }
        public string? OwnerNames { get; set; }

        IEnumerable<ValidationResult> IValidatableObject.Validate(ValidationContext validationContext)
        {
            List<ValidationResult> res = new List<ValidationResult>();
            if (ToDate != null && FromDate != null && ToDate - FromDate < TimeSpan.FromDays(0))
            {
                ValidationResult mss = new ValidationResult("The To Date should be later than the From Date.");
                res.Add(mss);

            }
            if ((FromScore != null && (FromScore < 0 || FromScore > 5)) || (ToScore != null && (ToScore < 0 || ToScore > 5)))
            {
                ValidationResult mss = new ValidationResult("The Score should between 0 to 5.");
                res.Add(mss);
            }
            if (ToScore != null && FromScore != null && ToScore - FromScore < 0)
            {
                ValidationResult mss = new ValidationResult("The To Score should be greater than the From Score.");
                res.Add(mss);

            }
            return res;
        }
    }
}
