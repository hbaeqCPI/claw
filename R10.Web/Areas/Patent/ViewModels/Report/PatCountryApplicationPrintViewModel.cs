using R10.Web.Areas.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Patent.ViewModels
{
    public class PatCountryApplicationPrintViewModel : ReportBaseViewModel
    {
        public string? IDs { get; set; }

        public bool PrintRemarks { get; set; }

        public bool PrintInventors { get; set; }

        public bool PrintAssignments { get; set; }

        public bool PrintPriorities { get; set; }

        public bool PrintGenDocs { get; set; }

        public bool PrintCosts { get; set; }

        public bool PrintDesCountries { get; set; }

        public bool PrintImage { get; set; }

        public bool PrintImageDetail { get; set; }

        public bool PrintActions { get; set; }

        public bool PrintActionsAll { get; set; }

        public bool PrintDueDateRemarks { get; set; }

        public bool PrintRelatedMatter { get; set; }

        public bool PrintRelatedTrademark { get; set; }

        public bool PrintProducts { get; set; }

        public bool PrintSubjectMatters  { get; set; }

        public bool PrintRelatedCases { get; set; }

        public bool PrintIDS { get; set; }

        public bool PrintLicensee { get; set; }
        public bool PrintTerminalDisclamer { get; set; }
        public bool PrintMap { get; set; }

        public bool PrintCustomFields { get; set; }
    }
}
