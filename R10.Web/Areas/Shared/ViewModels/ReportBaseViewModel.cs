using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class ReportBaseViewModel
    {
        public int ReportFormat { get; set; }
        public string? LanguageCode { get; set; }
    }
}
