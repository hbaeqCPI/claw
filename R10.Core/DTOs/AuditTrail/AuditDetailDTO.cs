using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.DTOs
{
    [Keyless]
    public class AuditDetailDTO
    {
        [Display(Name="Field Name")]
        public string? FieldName { get; set; }

        [Display(Name = "Old Value")]
        public string? OldValue { get; set; }

        [Display(Name = "New Value")]
        public string? NewValue { get; set; }
    }
}
