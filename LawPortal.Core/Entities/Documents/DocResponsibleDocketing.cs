using System.ComponentModel.DataAnnotations;

namespace LawPortal.Core.Entities.Documents
{    
    public class DocResponsibleDocketing : DocResponsibleDocketingDetail
    {
        public DocDocument? DocDocument { get; set; }
    }

    public class DocResponsibleDocketingDetail : BaseEntity
    {
        [Key]
        public int RespId { get; set; }

        public int DocId { get; set; }    

        public string? UserId { get; set; }

        public int? GroupId { get; set; }
    }
}
