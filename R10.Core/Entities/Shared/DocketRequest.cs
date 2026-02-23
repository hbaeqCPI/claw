using R10.Core.Entities.GeneralMatter;
using R10.Core.Entities.Patent;
using R10.Core.Entities.Trademark;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Core.Entities
{

    public class DocketRequest : BaseEntity
    {
        [Key]
        public int ReqId { get; set; }

        [Display(Name = "Request Type")]
        public string? RequestType { get; set; }

        
        public int? ActionTypeId { get; set; }

        [Display(Name = "Responsible")]
        public string? ResponsibleId { get; set; }

        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }

        public DateTime? DueDate { get; set; }

        public string? DocFile { get; set; }
        public int? FileId { get; set; }
        public string? DriveItemId { get; set; }

        public DateTime? CompletedDate { get; set; }
        public string? CompletedBy { get; set; }

        [NotMapped]
        [Display(Name = "Doc File")]
        public string? CurrentDocFile { get; set; }
        
    }

    public class PatDocketRequest : DocketRequest
    {
        public int AppId { get; set; }
        public CountryApplication? CountryApplication { get; set; }
        public List<PatDocketRequestResp>? PatDocketRequestResps { get; set; }
    }

    public class TmkDocketRequest : DocketRequest
    {
        public int TmkId { get; set; }
        public TmkTrademark? TmkTrademark { get; set; }
        public List<TmkDocketRequestResp>? TmkDocketRequestResps { get; set; }
    }

    public class GMDocketRequest : DocketRequest
    {
        public int MatId { get; set; }
        public GMMatter? GMMatter { get; set; }
        public List<GMDocketRequestResp>? GMDocketRequestResps { get; set; }
    }

    public class PatDocketInvRequest : DocketRequest
    {
        public int InvId { get; set; }
        public Invention? Invention { get; set; }
    }

}
