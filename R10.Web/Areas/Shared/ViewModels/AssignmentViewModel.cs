using R10.Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class AssignmentViewModel : BaseEntity
    {

        public int ParentId { get; set; }
        public int HistoryId { get; set; }

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

        [StringLength(11)]
        [Display(Name = "Status")]
        [UIHint("AssignmentStatus")]
        public string? AssignmentStatus { get; set; }

        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }

        public string? DocFilePath { get; set; }
        public int? FileId { get; set; }

        [Display(Name = "Saved Doc")]
        public string? CurrentDocFile { get; set; }
        
        public List<int>? MultiAssignmentFrom { get; set; }
    }
}
