using System.ComponentModel.DataAnnotations;

namespace LawPortal.Core.Entities.Documents
{
    public class DocQuickEmailLog : DocQuickEmailLogDetail
    {
        public DocDocument? DocDocument { get; set; }
    }

    public class DocQuickEmailLogDetail : BaseEntity
    {
        [Key]
        public int DQLogId { get; set; }

        public int DocId { get; set; }     

        public int? LogID { get; set; }
    }
}
