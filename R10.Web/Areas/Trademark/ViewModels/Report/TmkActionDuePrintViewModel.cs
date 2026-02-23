using R10.Web.Areas.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Trademark.ViewModels
{
    public class TmkActionDuePrintViewModel : ReportBaseViewModel
    {
        public string? IDs { get; set; }

        public bool PrintActionDueRemarks { get; set; }

        public bool PrintDueDateRemarks { get; set; }

        public bool PrintImage { get; set; }

        public bool PrintImageDetail { get; set; }

        public bool PrintGenDocs { get; set; }
    }
}
