using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Entities.AMS
{
    public class AMSCostExportLog : BaseEntity
    {

        [Key]
        public int LogID { get; set; }

        [Required]
        public int DueID { get; set; }

        [Display(Name = "Exclude?")]
        public bool? Exclude { get; set; }

        [Display(Name = "Export Date")]
        public DateTime? ProcessDate { get; set; }

        public AMSDue? AMSDue { get; set; }
    }
}
