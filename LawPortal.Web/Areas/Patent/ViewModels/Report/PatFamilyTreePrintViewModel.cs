using LawPortal.Web.Areas.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LawPortal.Web.Areas.Patent.ViewModels
{
    public class PatFamilyTreePrintViewModel : ReportBaseViewModel
    {
        public string? ID { get; set; }
        public string? RecordType { get; set; }
        public string? FamilyNumber { get; set; }
    }
}
