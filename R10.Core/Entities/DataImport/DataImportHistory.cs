using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Core.Entities
{
    
    public class DataImportHistory
    {
        [Key]
        public int ImportId { get; set; }

        [Display(Name ="Date")]
        public DateTime ImportDate { get; set; }

        [Display(Name = "Filename")]
        public string? OrigFileName { get; set; }
        public string? FileName { get; set; }

        public int DataTypeId { get; set; }

        [Display(Name = "Records")]
        public int NoOfRecords { get; set; }

        [Display(Name = "Imported")]
        public int NoOfRecordsImported { get; set; }
        
        public string? Status { get; set; }
        public string? Options { get; set; }

        public string? Error { get; set; }

        [Display(Name = "Imported By")]
        public string? ImportedBy { get; set; }

        [Display(Name = "System Type")]
        public string? SystemType { get; set; }

        public DataImportType? DataType { get; set; }

        [Display(Name = "Status")]
        [NotMapped]
        public string? TranslatedStatus { get; set; }

    }
}
