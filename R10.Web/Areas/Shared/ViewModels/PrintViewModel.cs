using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class PrintViewModel : ReportBaseViewModel
    {
        public string? token { get; set; }
        public string? IDs { get; set; }
    }
}
