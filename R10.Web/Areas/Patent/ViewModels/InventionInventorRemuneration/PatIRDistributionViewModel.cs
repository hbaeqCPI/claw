using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Core.Entities.Patent
{
    public class PatIRDistributionViewModel : PatIRDistribution
    {
        [Display(Name = "Inventor")]
        public string? Inventor { get; set; }
        [NotMapped]
        [Display(Name = "Amount Deducted")]
        public double AmountDeducted { get; set; } = 0;
        [NotMapped]
        [Display(Name = "Cumulative Revenue")]
        public double? CumulativeRevenue { get; set; }
        [NotMapped]
        [Display(Name = "Staggered Revenue")]
        public double? StaggeredRevenue { get; set; }

    }
}
