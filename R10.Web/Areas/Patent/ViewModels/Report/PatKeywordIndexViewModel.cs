using R10.Web.Areas.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Patent.ViewModels.Report
{
    public class PatKeywordIndexViewModel : ReportBaseViewModel
    {
        public bool PrintOtherKeywords { get; set; }
        public bool PrintPriorityInfo { get; set; }
        public bool PrintShowCriteria { get; set; }
        public bool PrintShowCriteriaOnFirstPage { get; set; }
        public string? RespOffice { get; set; }
        public string? RespOffices { get; set; }
        public string? Keyword { get; set; }
        public string? Keywords { get; set; }
        public string? Client { get; set; }
        public string? Clients { get; set; }
        public string? ClientName { get; set; }
        public string? ClientNames { get; set; }
        public string? Attorney { get; set; }
        public string? Attorneys { get; set; }
        public string? AttorneyName { get; set; }
        public string? AttorneyNames { get; set; }
        public string? Product { get; set; }
        public string? Products { get; set; }
        
    }
}
