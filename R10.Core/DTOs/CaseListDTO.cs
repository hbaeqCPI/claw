using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Core.DTOs
{
    public class CaseListDTO
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
        public int IsNew { get; set; }
        public int IsPastDue { get; set; }
    }
}
