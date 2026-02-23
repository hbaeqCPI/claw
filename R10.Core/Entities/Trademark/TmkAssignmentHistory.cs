using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Core.Entities.Trademark
{
    public class TmkAssignmentHistory : BaseEntity
    {
        [Key]
        public int HistoryId { get; set; }

        public int TmkId { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "From")]
        public string? AssignmentFrom { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "To")]
        public string? AssignmentTo { get; set; }

        [Display(Name = "Date Recorded")]
        [Required]
        public DateTime? AssignmentDate { get; set; }

        [StringLength(8)]
        [Display(Name = "Reel")]
        public string? Reel { get; set; }

        [StringLength(8)]
        [Display(Name = "Frame")]
        public string? Frame { get; set; }

        [StringLength(20)]
        [Display(Name = "Status")]
        public string? AssignmentStatus { get; set; }

        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }

        public string? DocFilePath { get; set; }
        public int? FileId { get; set; }

        [NotMapped]
        public string? CurrentDocFile { get; set; }

        public TmkTrademark? TmkTrademark { get; set; }
        public TmkAssignmentStatus? TmkAssignmentStatus{ get; set; }

    }
}
