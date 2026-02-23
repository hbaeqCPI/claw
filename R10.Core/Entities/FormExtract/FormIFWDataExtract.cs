using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Core.Entities.FormExtract
{
    public class FormIFWDataExtract 
    {
        [Key]
        public int ExtractId { get; set; }

        public int DocTypeId { get; set; }
        public int IFWId { get; set; }
        public int TLDocId { get; set; }
        public int SequenceNo { get; set; }

        public string? FieldName { get; set; }
        public string? FieldData { get; set; }
        public double Confidence { get; set; }

        public int UsageId { get; set; }

        [StringLength(20)]
        [Display(Name = "Created By")]
        public string? CreatedBy { get; set; }

        [Display(Name = "Date Created")]
        public DateTime? DateCreated { get; set; }

        [Column(TypeName = "timestamp")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [MaxLength(8)]
        [ConcurrencyCheck]
        public byte[]? tStamp { get; set; }
    
        public FormIFWFieldUsage? FormIFWFieldUsage { get; set; }
        public int DocId { get; set; }
    }

}
