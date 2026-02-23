using R10.Web.Areas.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Trademark.ViewModels
{
    public class TmkConflictPrintViewModel : ReportBaseViewModel
    {
        public string? IDs { get; set; }

        public bool PrintRemarks { get; set; }

        public bool PrintGenDocs { get; set; }
    }
}
