using System;
using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities
{
    
    public class DataImportMapping
    {
        [Key]
        public int MappingId { get; set; }
        public int ImportId { get; set; }

        [Display(Name ="File Header")]
        public string? YourField { get; set; }

        [Display(Name = "CPI Field")]
        public string? CPIField { get; set; }
        public int DisplayOrder { get; set; }

    }
}
