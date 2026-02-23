using R10.Web.Areas.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Patent.ViewModels
{
    public class PatCostTrackingPrintViewModel: ReportBaseViewModel
    {
        public string? IDs { get; set; }

        public bool PrintRemarks { get; set; }

        public bool PrintImage { get; set; }

        public bool PrintImageDetail { get; set; }

        public bool PrintGenDocs { get; set; }
    }
}
