using System;
using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Clearance
{
    public class TmcList : TmcListDetail
    {
        public TmcClearance? Clearance { get; set; }
    }


    public class TmcListDetail : BaseEntity
    {
        [Key]
        public int ListId { get; set; }

        [Required]
        public int TmcId { get; set; }

        [Required, StringLength(255)]
        public string ListItem { get; set; }

        public string? ItemStatus { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [StringLength(15)]
        public string? Initial { get; set; }

        public int OrderOfEntry { get; set; }
    }
}