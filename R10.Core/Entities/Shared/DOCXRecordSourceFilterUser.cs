using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities
{
    public class DOCXRecordSourceFilterUser : DOCXRecordSourceFilterUserDetail
    {
        public DOCXRecordSource? DOCXRecordSource { get; set; }
    }

    public class DOCXRecordSourceFilterUserDetail : BaseEntity
    {
        [Key]
        public long UserFilterId { get; set; }

        public int DOCXFilterId { get; set; }

        public string?  UserEmail { get; set; }

        public string?  FilterSource { get; set; } = "U";            // fix to user filter; this table is used for letter gen in the back-end and may contain the fix letter filter at that time

        public int DOCXId { get; set; }

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
