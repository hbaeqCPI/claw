using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Patent
{
    
    public class PatCostTrackingImportError
    {
        [Key]
        public int ErrorLogId { get; set; }

        public int ImportId { get; set; }
        public int ErrorType { get; set; }

        [Display(Name = "Row")]
        public int Row { get; set; }

        [Display(Name = "Error")]
        public string? Error { get; set; }

    }
}
