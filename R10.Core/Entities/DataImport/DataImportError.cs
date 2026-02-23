using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities
{
    
    public class DataImportError
    {
        [Key]
        public int ErrorLogId { get; set; }

        public int ImportId { get; set; }
        public DataImportErrorType ErrorType { get; set; }

        [Display(Name = "Row")]
        public int Row { get; set; }

        [Display(Name = "Error")]
        public string? Error { get; set; }

    }

    public enum DataImportErrorType
    {
        Invalid,
        MaxLength,
        Null,
        DataType,
        Duplicate,
        Ignored
    }
}
