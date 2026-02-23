using R10.Web.Areas.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Patent.ViewModels
{
    public class PatCountryLawPrintViewModel : ReportBaseViewModel
    {
        public string? IDs { get; set; }

        public bool PrintLawActions { get; set; }

        public bool PrintExpirationTerms { get; set; }

        public bool PrintLawHighlights { get; set; }

        public bool PrintUserRemarks { get; set; }
    }
}
