using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.DTOs
{
    [Keyless]
    public class AuditHeaderDTO 
    {
        public int AudTrailId { get; set; }

        [Display(Name = "System Type")]
        public string? SystemType { get; set; }

        [Display(Name = "Table Name")]
        public string? TableName { get; set; }

        [Display(Name = "Tranx Type")]
        public string? TranxType { get; set; }

        [Display(Name = "Tranx Date")]
        public DateTime? TranxDate { get; set; }

        [Display(Name = "User Name")]
        public string? UserName { get; set; }


    }

    [Keyless]
    public class AuditLogPagedResult
    {
        public int TotalCount { get; set; }
        public List<AuditHeaderDTO>? Data { get; set; }
    }
}
