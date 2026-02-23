using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities
{
    public class LetterRecordSourceFilterUser : LetterRecordSourceFilterUserDetail
    {
        public LetterRecordSource? LetterRecordSource { get; set; }
    }

    public class LetterRecordSourceFilterUserDetail : BaseEntity
    {
        [Key]
        public long UserFilterId { get; set; }

        public int LetFilterId { get; set; }

        public string?  UserEmail { get; set; }

        public string?  FilterSource { get; set; } = "U";            // fix to user filter; this table is used for letter gen in the back-end and may contain the fix letter filter at that time

        public int LetId { get; set; }

        [Display(Name = "Data Source")]
        [Required]
        public int RecSourceId { get; set; }

        [StringLength(50)]
        [Display(Name = "Field Name")]
        public string?  FieldName { get; set; }

        public int FieldType { get; set; }

        [Display(Name = "Condition")]
        public string?  Operator { get; set; }

        [StringLength(50)]
        [Display(Name = "Data 1")]
        public string?  Operand1 { get; set; }

        [StringLength(50)]
        [Display(Name = "Data 2")]
        public string?  Operand2 { get; set; }


    }
}
