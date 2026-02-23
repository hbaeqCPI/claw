using R10.Web.Areas.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Trademark.ViewModels.Report
{
    public class TLUSTTABViewModel : ReportBaseViewModel
    {
        public string? Plaintiff { get; set; }
        public string? Defendant { get; set; }
        public string? TTABStatusesOp { get; set; }
        public string? TTABStatuses { get; set; }
        public string? CaseNumber { get; set; }
        public string? Client { get; set; }
        public string? ClientName { get; set; }
        public string? Attorney { get; set; }
        public string? AttorneyName { get; set; }
        public string? TrademarkName { get; set; }
        public string? TrademarkNames { get; set; }
        public string? RespOffice { get; set; }
        public string? RespOffices { get; set; }
        public bool PrintShowCriteria { get; set; }
        public bool PrintShowCriteriaOnFirstPage { get; set; }
    }
}
