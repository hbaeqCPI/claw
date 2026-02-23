using System;
using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Patent
{
    public class PatEPOAppLog : BaseEntity
    {
        [Key]
        public int KeyId { get; set; }
        public int LogId { get; set; }
        public int AppId { get; set; }
        public string? Procedure { get; set; }
        public string? IpOfficeCode { get; set; }
        public string? AppNumber { get; set; }
        public DateTime? FilDate { get; set; }
    }    
}
