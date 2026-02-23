using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Patent
{   
    public class EPODueDate : BaseEntity
    {
        [Key]
        public int EPODDId { get; set; }
        public int LogId { get; set; }
        
        public string? Procedure { get; set; }
        public string? IpOfficeCode { get; set; }
        public string? AppNumber { get; set; }
        public string? AppNumberMyEPO { get; set; }
        public DateTime? FilDate { get; set; }

        public string? TermKey { get; set; }
        public DateTime? DueDate { get; set; }
        public string? Actor { get; set; }        
    }
}
