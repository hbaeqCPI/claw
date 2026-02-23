using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Entities.Patent
{
    public class PatEPODocumentMapAct : BaseEntity
    {
        [Key]
        public int MapDueId { get; set; }

        [Display(Name = "Communication Code")]
        [StringLength(25)]
        public string? DocumentCode { get; set; }

        [Required]
        [StringLength(60)]
        [Display(Name = "Action Type")]
        public string ActionType { get; set; }

        [Required]
        [StringLength(60)]
        [Display(Name = "Action Due")]
        public string ActionDue { get; set; }

        [Display(Name = "Yr")]
        public int Yr { get; set; }

        [Display(Name = "Mo")]
        public int Mo { get; set; }

        [Display(Name = "Dy")]
        public int Dy { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Indicator")]
        public string? Indicator { get; set; }
    }
}
