using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Entities.Patent
{
    public class PatIREmployeePosition : BaseEntity
    {
        [Key]
        public int PositionId { get; set; }

        [StringLength(50)]
        [Display(Name = "Title")]
        [Required]
        public string? Position { get; set; }

        [StringLength(255)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "A Position")]
        public int? PositionA { get; set; }

        [Display(Name = "B Creating the task of the invention")]
        public int? PositionB { get; set; }

        [Display(Name = "C Solution of the problem")]
        public int? PositionC { get; set; }

        public List<PatInventor>? Inventors { get; set; }
        public List<PatInventorInv>? InventorInvs { get; set; }
    }
}
