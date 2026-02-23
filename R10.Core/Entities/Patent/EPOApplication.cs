using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Patent
{   
    public class EPOApplication : BaseEntity
    {
        [Key]
        public int EPOAppId { get; set; }
        public int LogId { get; set; }
        
        public string? AppProcedure { get; set; }
        public string? IpOfficeCode { get; set; }
        public string? AppNumber { get; set; }
        public string? AppNumberMyEPO { get; set; }
        public DateTime? FilDate { get; set; }

        public string? ApplicantFileRef { get; set; }
        public string? PortfolioId { get; set; }
        public string? PortfolioName { get; set; }
        public string? Procedure { get; set; }
    }
}
