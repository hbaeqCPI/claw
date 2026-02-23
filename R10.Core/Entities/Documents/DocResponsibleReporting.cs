using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Documents
{    
    public class DocResponsibleReporting : DocResponsibleReportingDetail
    {
        public DocDocument? DocDocument { get; set; }
    }

    public class DocResponsibleReportingDetail : BaseEntity
    {
        [Key]
        public int RRId { get; set; }

        public int DocId { get; set; }

        public string? UserId { get; set; }

        public int? GroupId { get; set; }
    }
}
