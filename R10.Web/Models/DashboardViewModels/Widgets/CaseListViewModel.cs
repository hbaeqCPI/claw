using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Models.DashboardViewModels
{
    public class CaseListViewModel
    {
        public int Id { get; set; }
        public string? IdType { get; set; }
        public string? System { get; set; }
        public string? Title { get; set; }
        public string? CaseNumber { get; set; }
        public string? Country { get; set; }
        public string? SubCase { get; set; }
        public string? Status { get; set; }
        public string? Action { get; set; }
        public DateTime? DueDate { get; set; }
        public string? Remarks { get; set; }
        public int IsNew { get; set; }
        public int IsPastDue { get; set; }
        public bool IsInstructable { get; set; }

        public string? String1 { get; set; }
        public string? String2 { get; set; }
        public string? String3 { get; set; }
        public DateTime? Date1 { get; set; }
        public DateTime? Date2 { get; set; }
    }
}
