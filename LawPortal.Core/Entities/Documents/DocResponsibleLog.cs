using System.ComponentModel.DataAnnotations;

namespace LawPortal.Core.Entities.Documents
{    
    public class DocResponsibleLog : DocResponsibleLogDetail
    {
        public DocDocument? DocDocument { get; set; }
    }

    public class DocResponsibleLogDetail : BaseEntity
    {
        [Key]
        public int LogId { get; set; }
        public int DocId { get; set; }
        public string? UserIds { get; set; }
        public string? GroupIds { get; set; }
        public DocRespLogType RespType { get; set; }
        public DocRespLogTransxType TransxType { get; set; }
    }
    
    public enum DocRespLogTransxType
    {
        Update = 0,
        Delete
    }

    public enum DocRespLogType
    {
        Docketing = 0,
        Reporting
    }
}
