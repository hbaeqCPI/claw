using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.DTOs
{
    [Keyless]
    public class QEFieldListDTO
    {
        [Display(Name = "Data Field/Tag Name")]
        public string? ColumnName { get; set; }
        [Display(Name = "Data Type")]
        public string? DataType { get; set; }
        [Display(Name = "Fixed Value")]
        public string? DataValue { get; set; }
        public string? FieldSource { get; set; }
        public int? CustomFieldSettingId { get; set; }
    }
}
