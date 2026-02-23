using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.DTOs
{
    [Keyless]
    public class AuditKeyDTO
    {
        [Display(Name ="Key Field")]
        public string? KeyField { get; set; }

        [Display(Name = "Key Value")]
        public string? KeyValue { get; set; }
    }
}
