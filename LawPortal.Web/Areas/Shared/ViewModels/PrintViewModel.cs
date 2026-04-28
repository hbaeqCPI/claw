using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LawPortal.Web.Areas.Shared.ViewModels
{
    public class PrintViewModel : ReportBaseViewModel
    {
        public string? token { get; set; }
        public string? IDs { get; set; }
    }
}
