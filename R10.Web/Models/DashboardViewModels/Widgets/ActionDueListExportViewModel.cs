using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Models.DashboardViewModels
{
    public class ActionDueListExportViewModel
    {
        public string Title { get; set; }
        public string CaseNumber { get; set; }
        public string Country { get; set; }
        public string SubCase { get; set; }
        public string Status { get; set; }
        public string ActionType { get; set; }
        public DateTime? BaseDate { get; set; }
    }
}
