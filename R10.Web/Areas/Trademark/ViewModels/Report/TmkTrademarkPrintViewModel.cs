using R10.Web.Areas.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Trademark.ViewModels
{
    public class TmkTrademarkPrintViewModel : ReportBaseViewModel
    {
        public string? IDs { get; set; }

        public bool PrintRemarks { get; set; }

        public bool PrintClassGoods { get; set; }

        public bool PrintKeywords { get; set; }

        public bool PrintAssignments { get; set; }

        public bool PrintConflicts { get; set; }

        public bool PrintGenDocs { get; set; }

        public bool PrintCosts { get; set; }

        public bool PrintDesCountries { get; set; }

        public bool PrintImage { get; set; }

        public bool PrintImageDetail { get; set; }

        public bool PrintActions { get; set; }

        public bool PrintActionsAll { get; set; }

        public bool PrintDueDateRemarks { get; set; }

        public bool PrintRelatedMatter { get; set; }

        public bool PrintRelatedPatent { get; set; }
        public bool PrintRelatedCases { get; set; }
        public bool PrintRelatedSearchRequest { get; set; }
        public bool PrintProducts { get; set; }
        public bool PrintMap { get; set; }
        public bool PrintCustomFields { get; set; }
    }
}
