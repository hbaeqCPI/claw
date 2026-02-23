using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities
{
    public class LetterCustomField : LetterCustomFieldDetail
    {
        public LetterDataSource? LetterDataSource { get; set; }
    }

    public class LetterCustomFieldDetail : BaseEntity
    {
        [Key] 
        public int CFId { get; set; }
        public int DataSourceId { get; set; }
        [Display(Name = "Field Name")]
        public string? FieldName { get; set; }

        [Display(Name = "Condition")]
        public string? Condition { get; set; }

        [Display(Name = "Value")]
        public int Value { get; set; }

        [Display(Name = "Period")]
        public string? Period { get; set; }
        [Display(Name = "Custom Merged Field Name")]
        public string? CustomFieldName { get; set; }
        public string? FieldSource { get; set; }
        public int? CustomFieldSettingId { get; set; }

    }
}
