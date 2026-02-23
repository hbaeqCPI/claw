using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Models.DashboardViewModels
{
    public class PTOTransactionExportViewModel
    {        
        public string CaseNumber { get; set; }
        public string Country { get; set; }
        public string SubCase { get; set; }
        public string Status { get; set; }
        public string Title { get; set; }
        public string Action { get; set; }
        public Nullable<DateTime> BaseDate { get; set; }
    }
}
