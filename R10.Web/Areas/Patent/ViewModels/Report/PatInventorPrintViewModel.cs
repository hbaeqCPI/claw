using R10.Web.Areas.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Patent.ViewModels
{
    public class PatInventorPrintViewModel : ReportBaseViewModel
    {
        public string? IDs { get; set; }

        public bool PrintRemarks { get; set; }

        public bool PrintAwards { get; set; }

        public bool PrintAwardRemarks { get; set; }

        public bool PrintChart { get; set; }
    }
}
