using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.ReportScheduler
{
    public class RSFrequencyType : BaseEntity
    {
        [Key]
        public int FreqTypeId { get; set; }

        [Required]
        [StringLength(20)]
        public string Frequency { get; set; }
    }
}
