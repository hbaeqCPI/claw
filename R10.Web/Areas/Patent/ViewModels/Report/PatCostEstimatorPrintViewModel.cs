using R10.Web.Areas.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Patent.ViewModels
{
    public class PatCostEstimatorPrintViewModel : ReportBaseViewModel
    {
        public string? IDs { get; set; }
        public bool PrintRemarks { get; set; }
        public int LayoutFormat { get; set; }
        public bool PrintChart { get; set; }
        public bool ShowBudget { get; set; }
        public bool PrintMap { get; set; }
        public bool PrintAnnuityCost { get; set; }
        public bool PrintAllQuestions { get; set; }
    }
}
