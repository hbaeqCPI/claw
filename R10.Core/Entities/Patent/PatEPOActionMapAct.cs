using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Entities.Patent
{
    public class PatEPOActionMapAct : BaseEntity
    {
        [Key]
        public int MapDueId { get; set; }

        public int TermId { get; set; }

        [Required]
        [StringLength(60)]
        [Display(Name = "Action Type")]
        public string ActionType { get; set; }

        [Required]
        [StringLength(60)]
        [Display(Name = "Action Due")]
        public string ActionDue { get; set; }        

        [Required]
        [StringLength(20)]
        [Display(Name = "Indicator")]
        public string? Indicator { get; set; }

        public EPODueDateTerm? EPODueDateTerm { get; set; }
    }
}
