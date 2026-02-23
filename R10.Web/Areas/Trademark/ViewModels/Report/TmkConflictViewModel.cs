using R10.Web.Areas.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Trademark.ViewModels.Report
{
    public class TmkConflictViewModel : ReportBaseViewModel, IValidatableObject
    {
        public int SortOrder { get; set; }
        public string? PrintGoods { get; set; }
        public bool PrintRemarks { get; set; }
        public bool PrintProducts { get; set; }
        public bool PrintCustomFields { get; set; }
        public bool PrintImage { get; set; }
        public bool PrintImageDetail { get; set; }
        public bool PrintShowCriteria { get; set; }
        public bool PrintShowCriteriaOnFirstPage { get; set; }
        public int DateType { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? Area { get; set; }
        public string? MarkType { get; set; }
        public string? CountriesOp { get; set; }
        public string? Countries { get; set; }
        public string? CountryName { get; set; }
        public string? CountryNames { get; set; }
        public string? ActiveSwitch { get; set; }
        public string? TrademarkStatusesOp { get; set; }
        public string? TrademarkStatuses { get; set; }
        public string? ConflictActiveSwitch { get; set; }
        public string? ConflictStatusesOp { get; set; }
        public string? ConflictStatuses { get; set; }
        public string? CaseTypesOp { get; set; }
        public string? CaseTypes { get; set; }
        public string? RespOffice { get; set; }
        public string? RespOffices { get; set; }
        public string? TrademarkName { get; set; }
        public string? TrademarkNames { get; set; }
        public string? Client { get; set; }
        public string? Clients { get; set; }
        public string? ClientName { get; set; }
        public string? ClientNames { get; set; }
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
        public string? OtherParty { get; set; }
        public string? OtherPartys { get; set; }
        public string? OtherPartyMark { get; set; }
        public string? OtherPartyMarks { get; set; }
        public string? Directions { get; set; }
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
    }
}
