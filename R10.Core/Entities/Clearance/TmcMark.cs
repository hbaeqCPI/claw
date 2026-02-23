using System;
using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Clearance
{
    public class TmcMark : TmcMarkDetail
    {
        public TmcClearance? Clearance { get; set; }
    }


    public class TmcMarkDetail : BaseEntity
    {
        [Key]
        public int MarkId { get; set; }

        [Required]
        public int TmcId { get; set; }

        [Required, StringLength(255)]
        public string MarkName { get; set; }

        [StringLength(25)]
        public string? MarkType { get; set; }

        [StringLength(10)]
        public string? Language { get; set; }

        [StringLength(255)]
        public string? Translation { get; set; }

        public string? ProposedUse { get; set; }

    }
}