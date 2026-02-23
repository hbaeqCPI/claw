using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.DTOs
{
    [Keyless]
    public class AuditReportDTO 
    {   
        [Display(Name = "Table Name")]
        public string? TableName { get; set; }

        [Display(Name = "User Name")]
        public string? UserName { get; set; }        

        [Display(Name = "Tranx Date")]
        public DateTime? TranxDate { get; set; }

        [Display(Name = "Tranx Type")]
        public string? TranxType { get; set; }

        [Display(Name = "Primary Key")]
        public string? PrimaryKey { get; set; }

        [Display(Name = "Updated Fields")]
        public string? UpdatedFields { get; set; }

        [Display(Name = "Old Values")]
        public string? OldValues { get; set; }

        [Display(Name = "New Values")]
        public string? NewValues { get; set; }
    }
}
