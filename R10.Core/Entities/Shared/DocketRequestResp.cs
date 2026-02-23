using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities
{  
    public class DocketRequestResp : BaseEntity
    {
        [Key]
        public int RespId { get; set; }

        public int ReqId { get; set; }    

        public string? UserId { get; set; }

        public int? GroupId { get; set; }
    }

    public class PatDocketRequestResp : DocketRequestResp
    {
        public PatDocketRequest? PatDocketRequest { get; set; }
    }

    public class TmkDocketRequestResp : DocketRequestResp
    {
        public TmkDocketRequest? TmkDocketRequest { get; set; }
    }

    public class GMDocketRequestResp : DocketRequestResp
    {
        public GMDocketRequest? GMDocketRequest { get; set; }
    }
}
