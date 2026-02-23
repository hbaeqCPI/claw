using R10.Core.Entities.GlobalSearch;
using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Documents
{
    public class DocVerificationSearchField : BaseEntity
    {
        [Key]
        public int KeyId { get; set; }
        public int FieldId { get; set; }
        public string? FieldLabel { get; set; }
        public bool IsEnabled { get; set; }
        public int EntryOrder { get; set; }

        public GSField GSField { get; set; }
    }
}
