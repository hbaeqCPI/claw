using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities
{
    public class DOCXRecordSourceFilter : DOCXRecordSourceFilterDetail
    {
        public DOCXRecordSource? DOCXRecordSource { get; set; }
    }

    public class DOCXRecordSourceFilterDetail: BaseEntity
    {
        [Key]
        public int DOCXFilterId { get; set; }

        [Display(Name="Data Source")]
        [Required]
        public int RecSourceId { get; set; }

        [StringLength(50)]
        [Display(Name="Field Name")]
        public string?  FieldName { get; set; }

        public int FieldType { get; set; }

        [Display(Name="Condition")]
        public string?  Operator { get; set; }

        [StringLength(50)]
        [Display(Name="Data 1")]
        public string?  Operand1 { get; set; }

        [StringLength(50)]
        [Display (Name ="Data 2")]
        public string?  Operand2 { get; set; }


    }
}
