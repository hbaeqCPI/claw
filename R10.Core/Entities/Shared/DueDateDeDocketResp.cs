// using R10.Core.Entities.GeneralMatter; // Removed during deep clean
using R10.Core.Entities.Patent;
using R10.Core.Entities.Trademark;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Core.Entities
{

    public class DueDateDeDocketResp : BaseEntity
    {
        [Key]
        public int RespId { get; set; }

        public int DeDocketId { get; set; }    

        public string? UserId { get; set; }

        public int? GroupId { get; set; }
    }

    public class PatDueDateDeDocketResp : DueDateDeDocketResp
    {
        public PatDueDateDeDocket? PatDueDateDeDocket { get; set; }
    }

    public class TmkDueDateDeDocketResp : DueDateDeDocketResp
    {
        public TmkDueDateDeDocket? TmkDueDateDeDocket { get; set; }
    }

    public class GMDueDateDeDocketResp : DueDateDeDocketResp
    {
//         public GMDueDateDeDocket? GMDueDateDeDocket { get; set; } // Removed during deep clean
    }
}
