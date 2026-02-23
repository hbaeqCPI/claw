using R10.Web.Areas.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Patent.ViewModels.Report
{
    public class PatInventorAwardsReportViewModel : ReportBaseViewModel
    {
        public int SortOrder { get; set; }
        public int PaidOption { get; set; }
        public bool PrintShowCriteria { get; set; }
        public bool PrintShowCriteriaOnFirstPage { get; set; }
        public int DateType { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? Countries { get; set; }
        public string? CountryName { get; set; }
        public string? CountryNames { get; set; }
        public string? AwardTypes { get; set; }
        public string? Inventor { get; set; }
        public string? Inventors { get; set; }
        public string? RespOffice { get; set; }
        public string? RespOffices { get; set; }
        public string? Client { get; set; }
        public string? Clients{ get; set; }
        public string? ClientName{ get; set; }
        public string? ClientNames{ get; set; }
    }
}
