using R10.Web.Areas.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Patent.ViewModels
{
    public class PatRemunerationPrintViewModel : ReportBaseViewModel
    {
        public string? IDs { get; set; }

        public bool PrintInventors { get; set; }

        public bool PrintInventorRemarks { get; set; }

        public bool PrintDistributions { get; set; }

        public bool PrintDistributionRemarks { get; set; }

        public bool PrintProductSales { get; set; }

        public bool PrintValuationMatrix { get; set; }
    }
}
