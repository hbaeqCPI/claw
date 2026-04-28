using LawPortal.Web.Areas.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LawPortal.Web.Areas.Trademark.ViewModels
{
    public class TmkCountryLawPrintViewModel : ReportBaseViewModel
    {
        public string? IDs { get; set; }

        public bool PrintLawActions { get; set; }

        public bool PrintLawHighlights { get; set; }

        public bool PrintUserRemarks { get; set; }
    }
}
