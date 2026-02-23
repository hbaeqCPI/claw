using R10.Web.Areas.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Patent.ViewModels.Report
{
    public class PatIDSCrossCheckViewModel : ReportBaseViewModel
    {
        public int? AppID { get; set; }
        [Required]
        public string? BaseCaseNumber { get; set; }
        [Required]
        public string? BaseCountry { get; set; }
        public string? BaseSubCase { get; set; }
        public int MatchMethod { get; set; }
        public string? CaseNumber { get; set; }
        public string? CaseNumbers { get; set; }
        public string? Country { get; set; }
        public string? Countries { get; set; }
        public string? CountryName { get; set; }
        public string? CountryNames { get; set; }
        public string? Client { get; set; }
        public string? Clients { get; set; }
        public string? ClientName { get; set; }
        public string? ClientNames { get; set; }
        public string? Attorney { get; set; }
        public string? Attorneys { get; set; }
        public string? AttorneyName { get; set; }
        public string? AttorneyNames { get; set; }
        public string? GroupArtUnit { get; set; }
        public string? Inventor { get; set; }
        public string? Inventors { get; set; }
        public string? Keyword { get; set; }
        public string? Keywords { get; set; }
        public bool ShowDiscrepancy { get; set; }
        public string? CaseTypesOp { get; set; }
        public string? ApplicationStatusesOp { get; set; }
        public string? CaseTypes { get; set; }
        public string? ApplicationStatuses { get; set; }
        public string? RespOffice { get; set; }
        public string? RespOffices { get; set; }
        public bool IncludeRelatedCases { get; set; }
    }
}
