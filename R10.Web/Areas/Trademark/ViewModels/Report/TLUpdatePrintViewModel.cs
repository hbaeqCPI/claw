using R10.Web.Areas.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Trademark.ViewModels.Report
{
    public class TLUpdatePrintViewModel : ReportBaseViewModel
    {
        public DateTime? LastWebUpdateFrom { get; set; }
        public DateTime? LastWebUpdateTo { get; set; }
        public string? TMSCaseNumber { get; set; }
        public string? TMSCountry { get; set; }
        public string? Client { get; set; }
        public string? ActionType { get; set; }
        public bool Exclude { get; set; }
        public bool ActiveSwitch { get; set; }
    }
}
