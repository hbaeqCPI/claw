using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Entities.Shared
{
    public class AssignmentHistoryWebSvc
    {
        [Required]
        [StringLength(50)]
        public string? AssignmentFrom { get; set; }

        [Required]
        [StringLength(50)]
        public string? AssignmentTo { get; set; }

        [Required]
        public DateTime? AssignmentDate { get; set; }

        [StringLength(8)]
        public string? Reel { get; set; }

        [StringLength(8)]
        public string? Frame { get; set; }

        [StringLength(20)]
        public string? AssignmentStatus { get; set; }
    }
}
