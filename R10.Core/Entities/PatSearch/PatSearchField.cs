using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Core.Entities.Patent
{
    public class PatSearchField //:BaseEntity
    {
        [Key]
        public int FieldId { get; set; }
        public string? FieldName { get; set; }
        public string? FieldLabel { get; set; }
        public bool IsEnabled { get; set; }
        public int EntryOrder { get; set; }

        [NotMapped]
        public bool Included { get; set; }
    }
}
