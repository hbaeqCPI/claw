using R10.Web.Areas.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Patent.ViewModels
{
    public class PatInventionPrintViewModel : ReportBaseViewModel
    {
        public string? IDs { get; set; }

        public bool PrintRemarks { get; set; }

        public bool PrintInventors { get; set; }

        public bool PrintAbstract { get; set; }

        public bool PrintImage { get; set; }

        public bool PrintPriorities { get; set; }

        public bool PrintImageDetail { get; set; }

        public bool PrintCountryApplications { get; set; }

        public bool PrintRelatedMatter { get; set; }

        public bool PrintRelatedInventions { get; set; }

        public bool PrintKeywords { get; set; }

        public bool PrintRelatedDisclosures { get; set; }

        public bool PrintGenDocs { get; set; }

        public bool PrintCustomFields { get; set; }

        public bool PrintCosts { get; set; }
        public bool PrintActions { get; set; }
        public bool PrintActionsAll { get; set; }
        public bool PrintDueDateRemarks { get; set; }
        public bool PrintProducts { get; set; }
    }
}
