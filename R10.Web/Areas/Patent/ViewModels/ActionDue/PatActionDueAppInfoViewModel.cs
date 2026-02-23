using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Patent.ViewModels
{
    public class PatActionDueAppInfoViewModel
    {
        public int AppId { get; set; }
        public string? CaseNumber { get; set; }
        public string? Country { get; set; }
        public string? CountryName { get; set; }
        public string? SubCase { get; set; }
        public string? CaseType { get; set; }
        public string? ApplicationStatus { get; set; }
        public string? AppNumber { get; set; }
        public DateTime? FilDate { get; set; }
    }
}
