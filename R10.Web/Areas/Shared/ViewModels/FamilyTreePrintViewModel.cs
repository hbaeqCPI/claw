using R10.Web.Areas.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class FamilyTreePrintViewModel : ReportBaseViewModel
    {
        public string? ID { get; set; }
        public string? RecordType { get; set; }
        public string? FamilyNumber { get; set; }
    }
}
