using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Documents
{
    public class DocType : BaseEntity
    {
        [Key]
        public int DocTypeId { get; set; }

        public string? DocTypeName { get; set; }
        public string? DocTypeImage { get; set; }
        public string? RegExFilter { get; set; }

        public int EvalOrder { get; set; }
    }
}
