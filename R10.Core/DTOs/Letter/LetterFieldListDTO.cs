using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace R10.Core.DTOs
{
    [Keyless]
    public class LetterFieldListDTO
    {
        [Display(Name="Data Field/Tag Name")]
        public string? ColumnName { get; set; }
        [Display(Name="Data Type")]
        public string? DataType { get; set; }
        [Display(Name = "Fixed Value")]
        public string? DataValue { get; set; }
        public string? FieldSource { get; set; }
        public int? CustomFieldSettingId { get; set; }
    }
}
